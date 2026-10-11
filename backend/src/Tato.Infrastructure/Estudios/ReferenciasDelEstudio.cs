using Microsoft.EntityFrameworkCore;
using Tato.Application.Estudios;
using Tato.Infrastructure.Persistencia;

namespace Tato.Infrastructure.Estudios;

/// <summary>
/// Implementación de <see cref="IReferenciasDelEstudio"/>. El filtro por estudio del DbContext (D-42) hace que
/// un Id de otro estudio no se encuentre.
/// </summary>
internal sealed class ReferenciasDelEstudio : IReferenciasDelEstudio
{
    private readonly TatoDbContext _db;
    private readonly IEstudioActual _estudioActual;

    public ReferenciasDelEstudio(TatoDbContext db, IEstudioActual estudioActual)
    {
        _db = db;
        _estudioActual = estudioActual;
    }

    public Task<bool> EstiloDisponibleAsync(Guid estiloId, CancellationToken cancellationToken) =>
        _db.Estilos.AnyAsync(e => e.Id == estiloId && e.Activo, cancellationToken);

    public Task<bool> ZonaCuerpoDisponibleAsync(Guid zonaCuerpoId, CancellationToken cancellationToken) =>
        _db.ZonasCuerpo.AnyAsync(z => z.Id == zonaCuerpoId && z.Activo, cancellationToken);

    public Task<bool> TatuadorExisteAsync(Guid tatuadorId, CancellationToken cancellationToken) =>
        _db.Tatuadores.AnyAsync(t => t.Id == tatuadorId, cancellationToken);

    public async Task<int> MaxFotosPorPublicacionAsync(CancellationToken cancellationToken)
    {
        // Estudios no tiene filtro por estudio (cada fila es un estudio): se filtra con el de la sesión.
        var estudioId = _estudioActual.EstudioId
            ?? throw new InvalidOperationException("No hay un estudio en la sesión.");

        return await _db.Estudios
            .Where(e => e.Id == estudioId)
            .Select(e => e.MaxFotosPorPublicacion)
            .SingleAsync(cancellationToken);
    }
}
