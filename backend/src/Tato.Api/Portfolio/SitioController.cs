using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;
using Tato.Application.Comun;
using Tato.Application.Portfolio;
using Tato.Domain.Portfolio;

namespace Tato.Api.Portfolio;

/// <summary>
/// Lo que lee el sitio público (doc 07, 7): sin sesión y de solo lectura. Solo sale lo visible y lo Publicado, y lo
/// demás responde 404, igual que lo que no existe.
/// <para>
/// Las páginas y las publicaciones se guardan en la caché de salida del servidor, que se vacía entera con cualquier
/// cambio desde el panel (<see cref="InvalidarCacheSitio"/>, D-57).
/// </para>
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/publico")]
public sealed class SitioController : ControllerBase
{
    /// <summary>
    /// La política de la caché de salida de estos endpoints. Se arma en PortfolioExtensions.
    /// </summary>
    public const string PoliticaCache = "Sitio";

    /// <summary>
    /// La etiqueta con la que se guarda en la caché todo lo del sitio, para vaciarlo de una vez.
    /// </summary>
    public const string EtiquetaCache = "sitio";

    private const string TipoWebp = "image/webp";

    private readonly OpcionesSitio _opciones;

    public SitioController(IOptions<OpcionesSitio> opciones)
    {
        _opciones = opciones.Value;
    }

    /// <summary>
    /// Una página visible por su dirección: el encabezado, el «Sobre» y las páginas relacionadas.
    /// </summary>
    [HttpGet("paginas/{direccion}")]
    [OutputCache(PolicyName = PoliticaCache)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginaDelSitio>> ObtenerPagina(
        string direccion,
        [FromServices] IConsultasDelSitio consultas,
        CancellationToken cancellationToken)
    {
        if (DireccionValida(direccion) is not { } valida)
            return NotFound();

        var pagina = await consultas.ObtenerPaginaAsync(valida, cancellationToken);

        return pagina is null ? NotFound() : ConCache(pagina);
    }

    /// <summary>
    /// Las publicaciones de la página, de un tipo, para la grilla de Trabajos o la lista del Blog y su «Ver más».
    /// Primero las fijadas y después de la más nueva a la más vieja (RN-BLG-13).
    /// </summary>
    [HttpGet("paginas/{direccion}/publicaciones")]
    [OutputCache(PolicyName = PoliticaCache)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Pagina<PublicacionDelSitio>>> ListarPublicaciones(
        string direccion,
        [FromServices] IConsultasDelSitio consultas,
        [FromQuery] TipoPublicacion tipo = TipoPublicacion.Trabajo,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanio = Paginacion.TamanioPorDefecto,
        CancellationToken cancellationToken = default)
    {
        if (DireccionValida(direccion) is not { } valida)
            return NotFound();

        var filtro = new FiltroPublicacionesDelSitio(tipo, new Paginacion(pagina, tamanio), _opciones.LargoExtracto);
        var lista = await consultas.ListarPublicacionesAsync(valida, filtro, cancellationToken);

        return lista is null ? NotFound() : ConCache(lista);
    }

    /// <summary>
    /// El detalle de una publicación de la página. 404 si es de otra página o no se ve.
    /// </summary>
    [HttpGet("paginas/{direccion}/publicaciones/{id:guid}")]
    [OutputCache(PolicyName = PoliticaCache)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicacionDelSitioDetalle>> ObtenerPublicacion(
        string direccion,
        Guid id,
        [FromServices] IConsultasDelSitio consultas,
        CancellationToken cancellationToken)
    {
        if (DireccionValida(direccion) is not { } valida)
            return NotFound();

        var publicacion = await consultas.ObtenerPublicacionAsync(valida, id, cancellationToken);

        return publicacion is null ? NotFound() : ConCache(publicacion);
    }

    /// <summary>
    /// Una versión de una foto: miniatura, galeria o detalle. Lleva «.webp» en la ruta porque Cloudflare cachea por
    /// extensión. No pasa por la caché de salida: los archivos ya los guardan Cloudflare y el navegador.
    /// </summary>
    [HttpGet("fotos/{fotoId:guid}/{tamanio}.webp")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK, TipoWebp)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LeerFoto(
        Guid fotoId,
        TamanioFoto tamanio,
        [FromServices] LeerFotoDelSitio leerFoto,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(tamanio))
            return NotFound();

        var contenido = await leerFoto.EjecutarAsync(fotoId, tamanio, cancellationToken);

        if (contenido is null)
            return NotFound();

        // La foto no cambia nunca, pero se puede ocultar: el caché la guarda solo un rato (D-57).
        Response.Headers.CacheControl = $"public, max-age={_opciones.SegundosCacheFotos}";

        return File(contenido, TipoWebp);
    }

    /// <summary>
    /// Una dirección mal formada o reservada no es ninguna página: 404, igual que una que no existe.
    /// </summary>
    private static DireccionPublica? DireccionValida(string direccion)
    {
        try
        {
            return DireccionPublica.Crear(direccion);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>
    /// Lo deja guardar al navegador un rato (D-57). Cloudflare no cachea JSON por defecto.
    /// </summary>
    private T ConCache<T>(T valor)
    {
        Response.Headers.CacheControl = $"public, max-age={_opciones.SegundosCacheDatos}";
        return valor;
    }
}
