using Microsoft.AspNetCore.Mvc;
using Tato.Application.Portfolio;

namespace Tato.Api.Portfolio;

/// <summary>
/// Cambios de estado de una publicación. Cada uno recibe la versión que tenía el panel y devuelve la nueva.
/// </summary>
[ApiController]
[Route("api/publicaciones/{id:guid}")]
public sealed class AccionesPublicacionController : ControllerBase
{
    private readonly AccionesPublicacion _acciones;

    public AccionesPublicacionController(AccionesPublicacion acciones)
    {
        _acciones = acciones;
    }

    [HttpPost("consentimiento")]
    public async Task<PublicacionVersionRespuesta> ConfirmarConsentimiento(
        Guid id,
        SolicitudVersion solicitud,
        CancellationToken cancellationToken) =>
        new(await _acciones.ConfirmarConsentimientoAsync(id, solicitud.Version, cancellationToken));

    [HttpPost("publicar")]
    public async Task<PublicacionVersionRespuesta> Publicar(
        Guid id,
        SolicitudVersion solicitud,
        CancellationToken cancellationToken) =>
        new(await _acciones.PublicarAsync(id, solicitud.Version, cancellationToken));

    [HttpPost("ocultar")]
    public async Task<PublicacionVersionRespuesta> Ocultar(
        Guid id,
        SolicitudVersion solicitud,
        CancellationToken cancellationToken) =>
        new(await _acciones.OcultarAsync(id, solicitud.Version, cancellationToken));
}

/// <summary>
/// La versión que tenía el panel al abrir la publicación.
/// </summary>
public sealed record SolicitudVersion(string Version);
