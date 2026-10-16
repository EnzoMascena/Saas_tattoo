using Microsoft.EntityFrameworkCore;
using Tato.Application.Comun;
using Tato.Application.Portfolio;
using Tato.Domain.Portfolio;
using Tato.Infrastructure.Persistencia;

namespace Tato.Infrastructure.Portfolio;

/// <summary>
/// Lecturas de publicaciones para el panel. Proyecta directo a los records de Application: la base devuelve solo
/// las columnas necesarias y no trackea nada (AsNoTracking). El filtro por estudio (D-42) lo aplica el DbContext.
/// </summary>
internal sealed class ConsultasPublicaciones : IConsultasPublicaciones
{
    private readonly TatoDbContext _db;

    public ConsultasPublicaciones(TatoDbContext db)
    {
        _db = db;
    }

    public async Task<Pagina<PublicacionResumen>> ListarAsync(
        FiltroPublicaciones filtro,
        CancellationToken cancellationToken)
    {
        var consulta = _db.Publicaciones.AsNoTracking();

        if (filtro.Estado is { } estado)
            consulta = consulta.Where(p => p.Estado == estado);

        // Primero el total (para la paginación), después solo la página pedida.
        var total = await consulta.CountAsync(cancellationToken);

        var items = await consulta
            .OrderByDescending(p => p.ActualizadaEn)
            .Skip(filtro.Paginacion.Saltear)
            .Take(filtro.Paginacion.TamanioPagina)
            .Select(p => new PublicacionResumen(
                p.Id,
                p.Titulo,
                p.Estado,
                p.Destacada,
                p.FechaTrabajo,
                p.ActualizadaEn,
                p.Fotos.OrderBy(f => f.Orden).Select(f => (Guid?)f.Id).FirstOrDefault(),
                p.Fotos.Count()))
            .ToListAsync(cancellationToken);

        return new Pagina<PublicacionResumen>(
            items,
            filtro.Paginacion.NumeroPagina,
            filtro.Paginacion.TamanioPagina,
            total);
    }

    public async Task<PublicacionDetalle?> ObtenerAsync(Guid id, CancellationToken cancellationToken)
    {
        var fila = await _db.Publicaciones
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Titulo,
                p.Descripcion,
                p.FechaTrabajo,
                p.EstiloId,
                p.ZonaCuerpoId,
                p.TatuadorId,
                p.Destacada,
                p.Estado,
                TieneConsentimiento = p.ConsentimientoConfirmadoEn != null,
                p.PublicadaEn,
                p.ActualizadaEn,
                // La columna de concurrencia es shadow property: se lee con EF.Property.
                Version = EF.Property<byte[]>(p, PublicacionConfiguration.ColumnaVersion),
                Fotos = p.Fotos
                    .OrderBy(f => f.Orden)
                    .Select(f => new FotoDePublicacion(f.Id, f.Orden, f.Ancho, f.Alto))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Null si no existe o es de otro estudio: el filtro lo oculta igual que si no existiera.
        if (fila is null)
            return null;

        return new PublicacionDetalle(
            fila.Id,
            fila.Titulo,
            fila.Descripcion,
            fila.FechaTrabajo,
            fila.EstiloId,
            fila.ZonaCuerpoId,
            fila.TatuadorId,
            fila.Destacada,
            fila.Estado,
            fila.TieneConsentimiento,
            fila.PublicadaEn,
            fila.ActualizadaEn,
            Convert.ToBase64String(fila.Version),
            fila.Fotos);
    }
}