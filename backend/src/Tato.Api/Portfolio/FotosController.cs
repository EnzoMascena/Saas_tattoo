using Microsoft.AspNetCore.Mvc;
using Tato.Application.Portfolio;

namespace Tato.Api.Portfolio;

/// <summary>
/// Subida de fotos desde el panel y entrega de sus versiones optimizadas (RN-BLG-05).
/// </summary>
[ApiController]
public sealed class FotosController : ControllerBase
{
    /// <summary>
    /// Formato de todas las versiones: el procesador las guarda en WebP.
    /// </summary>
    private const string TipoWebp = "image/webp";

    /// <summary>
    /// Sube una foto como formulario (multipart/form-data) y la agrega al final de la publicación.
    /// </summary>
    /// <param name="archivo">La foto, tal como la saca el celular.</param>
    /// <param name="version">La versión que tenía el panel.</param>
    [HttpPost("api/publicaciones/{id:guid}/fotos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<FotoSubida>> Subir(
        Guid id,
        IFormFile archivo,
        [FromForm] string version,
        [FromServices] SubirFoto subirFoto,
        CancellationToken cancellationToken)
    {
        await using var contenido = archivo.OpenReadStream();
        var resultado = await subirFoto.EjecutarAsync(id, contenido, version, cancellationToken);

        return CreatedAtAction(
            nameof(Leer), new { fotoId = resultado.Foto.Id, tamanio = TamanioFoto.Detalle }, resultado);
    }

    /// <summary>
    /// Devuelve una versión de la foto: miniatura, galeria o detalle.
    /// </summary>
    [HttpGet("api/fotos/{fotoId:guid}/{tamanio}")]
    [ProducesResponseType(typeof(Stream), StatusCodes.Status200OK, TipoWebp)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Leer(
        Guid fotoId,
        TamanioFoto tamanio,
        [FromServices] LeerFoto leerFoto,
        CancellationToken cancellationToken)
    {
        // Un número que no es ningún tamaño («/99») también se puede convertir al enum: se rechaza acá.
        if (!Enum.IsDefined(tamanio))
            return NotFound();

        var contenido = await leerFoto.EjecutarAsync(fotoId, tamanio, cancellationToken);

        if (contenido is null)
            return NotFound();

        // Una foto no cambia nunca (reemplazarla es subir otra, con otro Id): el navegador la guarda un año.
        // «private»: es del panel, con sesión; no la guarda ningún caché compartido.
        Response.Headers.CacheControl = "private, max-age=31536000, immutable";

        // File cierra el stream cuando termina de mandarlo.
        return File(contenido, TipoWebp);
    }
}
