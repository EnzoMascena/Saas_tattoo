using Microsoft.EntityFrameworkCore;
using Tato.Application.Comun;
using Tato.Application.Portfolio;
using Tato.Domain.Portfolio;
using Tato.Infrastructure.Persistencia;

namespace Tato.Infrastructure.Portfolio;

/// <summary>
/// Lecturas del sitio público, sin sesión. Proyecta directo a los records de Application y no trackea nada.
/// <para>
/// Sin sesión no hay estudio, y el filtro por estudio (D-42) dejaría todo afuera. Por eso estas consultas lo saltean
/// a propósito, y solo a ese filtro, y filtran a mano: la página por su dirección, que es única en toda la
/// plataforma, y solo si está visible; sus publicaciones, por el estudio y el artista de la página, y solo las
/// Publicadas (RN-BLG-01, RN-BLG-12). Los tests de ConsultasDelSitioTests prueban que no se vea nada de más.
/// </para>
/// </summary>
internal sealed class ConsultasDelSitio : IConsultasDelSitio
{
    private readonly TatoDbContext _db;

    public ConsultasDelSitio(TatoDbContext db)
    {
        _db = db;
    }

    private IQueryable<PaginaPublica> PaginasVisibles =>
        _db.PaginasPublicas.IgnoreQueryFilters([TatoDbContext.FiltroEstudio]).AsNoTracking().Where(p => p.Visible);

    public async Task<PaginaDelSitio?> ObtenerPaginaAsync(
        DireccionPublica direccion,
        CancellationToken cancellationToken)
    {
        var pagina = await PaginasVisibles
            .Where(p => p.Direccion == direccion)
            .Select(p => new
            {
                p.EstudioId,
                p.TatuadorId,
                p.Nombre,
                p.Bio,
                p.Foto,
                p.Instagram,
                p.WhatsApp,
                p.Sobre,
                p.FotoSobre,
                p.Ubicacion,
                p.Horarios,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (pagina is null)
            return null;

        // Las relacionadas son siempre del mismo estudio (doc 07, 7.4): la del estudio trae las de sus artistas, y la de
        // un artista, la del estudio.
        var esDelEstudio = pagina.TatuadorId is null;
        var delMismoEstudio = PaginasVisibles.Where(p => p.EstudioId == pagina.EstudioId);
        var relacionadas = esDelEstudio
            ? delMismoEstudio.Where(p => p.TatuadorId != null)
            : delMismoEstudio.Where(p => p.TatuadorId == null);

        var filas = await relacionadas
            .OrderBy(p => p.Nombre)
            .Select(p => new { p.Direccion, p.Nombre, p.Foto })
            .ToListAsync(cancellationToken);

        var enlaces = filas
            .Select(p => new EnlacePagina(p.Direccion.Valor, p.Nombre, FotoConMedidas.De(p.Foto)))
            .ToList();

        return new PaginaDelSitio(
            direccion.Valor,
            esDelEstudio,
            pagina.Nombre,
            pagina.Bio,
            FotoConMedidas.De(pagina.Foto),
            pagina.Instagram,
            pagina.WhatsApp,
            pagina.Sobre,
            FotoConMedidas.De(pagina.FotoSobre),
            pagina.Ubicacion,
            pagina.Horarios,
            Estudio: esDelEstudio ? null : enlaces.FirstOrDefault(),
            Artistas: esDelEstudio ? enlaces : []);
    }

    public async Task<Pagina<PublicacionDelSitio>?> ListarPublicacionesAsync(
        DireccionPublica direccion,
        FiltroPublicacionesDelSitio filtro,
        CancellationToken cancellationToken)
    {
        var consulta = await PublicadasDeAsync(direccion, cancellationToken);

        if (consulta is null)
            return null;

        consulta = consulta.Where(p => p.Tipo == filtro.Tipo);

        var total = await consulta.CountAsync(cancellationToken);
        var conTexto = filtro.Tipo == TipoPublicacion.Blog;

        // RN-BLG-13: primero las fijadas y después de la publicada más recientemente a la más vieja. El Id desempata,
        // para que el «Ver más» no repita ni saltee publicaciones con la misma fecha.
        var filas = await consulta
            .OrderByDescending(p => p.Destacada)
            .ThenByDescending(p => p.PublicadaEn)
            .ThenBy(p => p.Id)
            .Skip(filtro.Paginacion.Saltear)
            .Take(filtro.Paginacion.TamanioPagina)
            .Select(p => new
            {
                p.Id,
                p.Titulo,
                p.PublicadaEn,
                Descripcion = conTexto ? p.Descripcion : null,
                Portada = p.Fotos
                    .OrderBy(f => f.Orden)
                    .Select(f => new FotoConMedidas(f.Id, f.Ancho, f.Alto))
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        // Una publicada tiene siempre fecha de publicación y al menos una foto (RN-BLG-04).
        var items = filas
            .Select(f => new PublicacionDelSitio(
                f.Id,
                f.Titulo,
                f.PublicadaEn!.Value,
                Extracto(f.Descripcion, filtro.LargoExtracto),
                f.Portada!))
            .ToList();

        return new Pagina<PublicacionDelSitio>(
            items, filtro.Paginacion.NumeroPagina, filtro.Paginacion.TamanioPagina, total);
    }

    public async Task<PublicacionDelSitioDetalle?> ObtenerPublicacionAsync(
        DireccionPublica direccion,
        Guid id,
        CancellationToken cancellationToken)
    {
        var consulta = await PublicadasDeAsync(direccion, cancellationToken);

        if (consulta is null)
            return null;

        var fila = await consulta
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Tipo,
                p.Titulo,
                p.Descripcion,
                p.PublicadaEn,
                Fotos = p.Fotos
                    .OrderBy(f => f.Orden)
                    .Select(f => new FotoConMedidas(f.Id, f.Ancho, f.Alto))
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return fila is null
            ? null
            : new PublicacionDelSitioDetalle(
                fila.Id, fila.Tipo, fila.Titulo, fila.Descripcion, fila.PublicadaEn!.Value, fila.Fotos);
    }

    public async Task<UbicacionFoto?> UbicarFotoAsync(Guid fotoId, CancellationToken cancellationToken)
    {
        // De una publicación Publicada cuya página se ve: la de su artista o, sin artista, la del estudio
        // (RN-BLG-12). Las dos comparaciones de TatuadorId aceptan null: EF las traduce con IS NULL.
        var paginasVisibles = PaginasVisibles;
        var deUnaPublicacion = await _db.Publicaciones
            .IgnoreQueryFilters([TatoDbContext.FiltroEstudio])
            .Where(p => p.Estado == EstadoPublicacion.Publicada && p.Fotos.Any(f => f.Id == fotoId))
            .Where(p => paginasVisibles.Any(pagina =>
                pagina.EstudioId == p.EstudioId && pagina.TatuadorId == p.TatuadorId))
            .Select(p => (Guid?)p.EstudioId)
            .FirstOrDefaultAsync(cancellationToken);

        if (deUnaPublicacion is { } estudioDeLaPublicacion)
            return new UbicacionFoto(estudioDeLaPublicacion, EsDePagina: false);

        // O la foto o logo, o la de «Sobre», de una página visible (R-16).
        var deUnaPagina = await PaginasVisibles
            .Where(p => p.Foto!.Id == fotoId || p.FotoSobre!.Id == fotoId)
            .Select(p => (Guid?)p.EstudioId)
            .FirstOrDefaultAsync(cancellationToken);

        return deUnaPagina is { } estudioDeLaPagina ? new UbicacionFoto(estudioDeLaPagina, EsDePagina: true) : null;
    }

    /// <summary>
    /// El comienzo del texto, cortado en el último espacio antes del largo pedido y con «…» si sigue.
    /// </summary>
    internal static string? Extracto(string? texto, int largo)
    {
        if (texto is null || texto.Length <= largo)
            return texto;

        var corte = texto.LastIndexOf(' ', largo);

        // Sin espacios cerca (una palabra larguísima o un link), se corta en el largo justo.
        if (corte < largo / 2)
            corte = largo;

        return texto[..corte].TrimEnd() + "…";
    }

    /// <summary>
    /// Las publicaciones Publicadas de la página con esa dirección, o null si la página no existe o no se ve. Una
    /// publicación es de la página de su artista o, sin artista, de la del estudio (RN-BLG-12): por eso se compara
    /// también el estudio, porque las páginas de todos los estudios tienen el artista vacío.
    /// </summary>
    private async Task<IQueryable<Publicacion>?> PublicadasDeAsync(
        DireccionPublica direccion,
        CancellationToken cancellationToken)
    {
        var pagina = await PaginasVisibles
            .Where(p => p.Direccion == direccion)
            .Select(p => new { p.EstudioId, p.TatuadorId })
            .FirstOrDefaultAsync(cancellationToken);

        if (pagina is null)
            return null;

        return _db.Publicaciones
            .IgnoreQueryFilters([TatoDbContext.FiltroEstudio])
            .AsNoTracking()
            .Where(p => p.EstudioId == pagina.EstudioId
                && p.TatuadorId == pagina.TatuadorId
                && p.Estado == EstadoPublicacion.Publicada);
    }
}
