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

    public ReferenciasDelEstudio(TatoDbContext db)
    {
        _db = db;
    }

    public Task<bool> EstiloDisponibleAsync(Guid estiloId, CancellationToken cancellationToken) =>
        _db.Estilos.AnyAsync(e => e.Id == estiloId && e.Activo, cancellationToken);

    public Task<bool> ZonaCuerpoDisponibleAsync(Guid zonaCuerpoId, CancellationToken cancellationToken) =>
        _db.ZonasCuerpo.AnyAsync(z => z.Id == zonaCuerpoId && z.Activo, cancellationToken);

    public Task<bool> TatuadorExisteAsync(Guid tatuadorId, CancellationToken cancellationToken) =>
        _db.Tatuadores.AnyAsync(t => t.Id == tatuadorId, cancellationToken);
}
