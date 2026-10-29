using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tato.Application.Portfolio;

namespace Tato.Api.Portfolio;

/// <summary>
/// Lo que lee el sitio público (doc 07, 7): sin sesión y de solo lectura. Solo sale lo visible y lo Publicado, y lo
/// demás responde 404, igual que lo que no existe.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/publico")]
public sealed class SitioController : ControllerBase
{
    private const string TipoWebp = "image/webp";

    private readonly OpcionesSitio _opciones;

    public SitioController(IOptions<OpcionesSitio> opciones)
    {
        _opciones = opciones.Value;
    }

    /// <summary>
    /// Una versión de una foto: miniatura, galeria o detalle. Lleva «.webp» en la ruta porque Cloudflare cachea por
    /// extensión.
    /// </summary>
    [HttpGet("fotos/{fotoId:guid}/{tamanio}.webp")]
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
}
