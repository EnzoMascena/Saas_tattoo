using Microsoft.EntityFrameworkCore;
using Tato.Application.Portfolio;
using Tato.Domain.Portfolio;
using Tato.Infrastructure.Persistencia;

namespace Tato.Infrastructure.Portfolio;

/// <summary>
/// Lecturas de páginas públicas para el panel. Proyecta directo a <see cref="PaginaDetalle"/> y no trackea nada.
/// El filtro por estudio (D-42) lo aplica el DbContext.
/// </summary>
internal sealed class ConsultasPaginas : IConsultasPaginas
{
    private readonly TatoDbContext _db;

    public ConsultasPaginas(TatoDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PaginaDetalle>> ListarAsync(CancellationToken cancellationToken) =>
        await LeerAsync(_db.PaginasPublicas, cancellationToken);

    public async Task<PaginaDetalle?> ObtenerAsync(Guid id, CancellationToken cancellationToken) =>
        (await LeerAsync(_db.PaginasPublicas.Where(p => p.Id == id), cancellationToken)).SingleOrDefault();

    private static async Task<List<PaginaDetalle>> LeerAsync(
        IQueryable<PaginaPublica> paginas,
        CancellationToken cancellationToken)
    {
        var filas = await paginas
            .AsNoTracking()
            .Select(p => new
            {
                p.Id,
                p.TatuadorId,
                p.Direccion,
                p.Nombre,
                p.Bio,
                p.Instagram,
                p.WhatsApp,
                p.Sobre,
                p.Ubicacion,
                p.Horarios,
                p.Foto,
                p.FotoSobre,
                p.Visible,
                p.ActualizadaEn,
                // La columna de concurrencia es shadow property: se lee con EF.Property.
                Version = EF.Property<byte[]>(p, PublicacionConfiguration.ColumnaVersion),
            })
            .ToListAsync(cancellationToken);

        // La dirección y las fotos llegan como objetos de valor: al record van sus datos.
        return
        [
            .. filas.Select(f => new PaginaDetalle(
                f.Id,
                EsDelEstudio: f.TatuadorId is null,
                f.TatuadorId,
                f.Direccion.Valor,
                f.Nombre,
                f.Bio,
                f.Instagram,
                f.WhatsApp,
                f.Sobre,
                f.Ubicacion,
                f.Horarios,
                ConMedidas(f.Foto),
                ConMedidas(f.FotoSobre),
                f.Visible,
                f.ActualizadaEn,
                Convert.ToBase64String(f.Version))),
        ];
    }

    private static FotoConMedidas? ConMedidas(FotoPagina? foto) =>
        foto is null ? null : new FotoConMedidas(foto.Id, foto.Ancho, foto.Alto);
}
