using Microsoft.AspNetCore.Mvc;
using Tato.Application.Portfolio;

namespace Tato.Api.Portfolio;

/// <summary>
/// Cambios sobre las fotos de una publicación. Cada uno recibe la versión que tenía el panel y devuelve la nueva.
/// </summary>
[ApiController]
[Route("api/publicaciones/{id:guid}/fotos")]
public sealed class AccionesFotosController : ControllerBase
{
    private readonly AccionesFotos _acciones;

    public AccionesFotosController(AccionesFotos acciones)
    {
        _acciones = acciones;
    }

    /// <summary>
    /// Quita la foto y borra sus archivos. Es POST y no DELETE para mandar la versión en el cuerpo, como las
    /// demás acciones.
    /// </summary>
    [HttpPost("{fotoId:guid}/quitar")]
    public async Task<PublicacionVersionRespuesta> Quitar(
        Guid id,
        Guid fotoId,
        SolicitudVersion solicitud,
        CancellationToken cancellationToken) =>
        new(await _acciones.QuitarAsync(id, fotoId, solicitud.Version, cancellationToken));

    /// <summary>
    /// Cambia el orden de todas las fotos. La primera de la lista queda como portada.
    /// </summary>
    [HttpPut("orden")]
    public async Task<PublicacionVersionRespuesta> Reordenar(
        Guid id,
        SolicitudOrdenFotos solicitud,
        CancellationToken cancellationToken) =>
        new(await _acciones.ReordenarAsync(id, solicitud.FotoIds, solicitud.Version, cancellationToken));

    [HttpPost("{fotoId:guid}/portada")]
    public async Task<PublicacionVersionRespuesta> ElegirPortada(
        Guid id,
        Guid fotoId,
        SolicitudVersion solicitud,
        CancellationToken cancellationToken) =>
        new(await _acciones.ElegirPortadaAsync(id, fotoId, solicitud.Version, cancellationToken));
}

/// <summary>
/// El orden nuevo de las fotos (cada una una sola vez) y la versión que tenía el panel.
/// </summary>
public sealed record SolicitudOrdenFotos(IReadOnlyList<Guid> FotoIds, string Version);
