using Microsoft.AspNetCore.Mvc;
using Tato.Application.Portfolio;
using Tato.Domain.Portfolio;

namespace Tato.Api.Portfolio;

/// <summary>
/// «Mi página» del panel privado: la página del estudio y la de cada artista (doc 07, sección 8). Exige sesión y
/// todo es del estudio de la sesión (D-42). Los cambios reciben la versión que tenía el panel y devuelven la nueva.
/// </summary>
[ApiController]
[Route("api/paginas")]
public sealed class PaginasController : ControllerBase
{
    /// <summary>
    /// Las páginas que gestiona la persona de la sesión: primero la suya de artista y, si es Dueño, la del estudio.
    /// </summary>
    [HttpGet]
    public Task<IReadOnlyList<PaginaDetalle>> Listar(
        [FromServices] MisPaginas misPaginas,
        CancellationToken cancellationToken) =>
        misPaginas.EjecutarAsync(cancellationToken);

    /// <summary>
    /// Una página del estudio. 404 si no existe o es de otro estudio.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaginaDetalle>> Obtener(
        Guid id,
        [FromServices] IConsultasPaginas consultas,
        CancellationToken cancellationToken)
    {
        var detalle = await consultas.ObtenerAsync(id, cancellationToken);

        return detalle is null ? NotFound() : detalle;
    }

    /// <summary>
    /// Crea la página del estudio (delEstudio: true) o la propia de artista. Nace oculta.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PaginaCreadaRespuesta>> Crear(
        SolicitudPagina solicitud,
        [FromServices] CrearPagina crearPagina,
        CancellationToken cancellationToken)
    {
        var id = await crearPagina.EjecutarAsync(solicitud.DelEstudio, solicitud.ADatos(), cancellationToken);

        return CreatedAtAction(nameof(Obtener), new { id }, new PaginaCreadaRespuesta(id));
    }

    /// <summary>
    /// Reemplaza los datos. La dirección no cambia, aunque cambie el nombre (RN-BLG-07).
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<PaginaVersionRespuesta> Editar(
        Guid id,
        SolicitudEditarPagina solicitud,
        [FromServices] AccionesPagina acciones,
        CancellationToken cancellationToken) =>
        new(await acciones.EditarAsync(id, solicitud.ADatos(), solicitud.Version, cancellationToken));

    /// <summary>
    /// Cambia la dirección a propósito. 422 si otra página ya la usa (RN-BLG-07).
    /// </summary>
    [HttpPut("{id:guid}/direccion")]
    public async Task<PaginaVersionRespuesta> CambiarDireccion(
        Guid id,
        SolicitudDireccion solicitud,
        [FromServices] AccionesPagina acciones,
        CancellationToken cancellationToken) =>
        new(await acciones.CambiarDireccionAsync(id, solicitud.Direccion, solicitud.Version, cancellationToken));

    [HttpPost("{id:guid}/mostrar")]
    public async Task<PaginaVersionRespuesta> Mostrar(
        Guid id,
        SolicitudVersion solicitud,
        [FromServices] AccionesPagina acciones,
        CancellationToken cancellationToken) =>
        new(await acciones.MostrarAsync(id, solicitud.Version, cancellationToken));

    [HttpPost("{id:guid}/ocultar")]
    public async Task<PaginaVersionRespuesta> Ocultar(
        Guid id,
        SolicitudVersion solicitud,
        [FromServices] AccionesPagina acciones,
        CancellationToken cancellationToken) =>
        new(await acciones.OcultarAsync(id, solicitud.Version, cancellationToken));
}

/// <summary>
/// Datos de una página nueva. No trae el estudio ni el artista: salen de la sesión.
/// </summary>
/// <param name="DelEstudio">true para la página del estudio; false para la propia de artista.</param>
public sealed record SolicitudPagina(
    bool DelEstudio,
    string Nombre,
    string? Bio,
    string? Instagram,
    string? WhatsApp,
    string? Sobre,
    string? Ubicacion,
    string? Horarios)
{
    internal DatosPagina ADatos() => new(Nombre, Bio, Instagram, WhatsApp, Sobre, Ubicacion, Horarios);
}

/// <summary>
/// Los datos nuevos de la página, más la versión que tenía el panel al abrirla. Reemplaza todos los datos.
/// </summary>
public sealed record SolicitudEditarPagina(
    string Nombre,
    string? Bio,
    string? Instagram,
    string? WhatsApp,
    string? Sobre,
    string? Ubicacion,
    string? Horarios,
    string Version)
{
    internal DatosPagina ADatos() => new(Nombre, Bio, Instagram, WhatsApp, Sobre, Ubicacion, Horarios);
}

public sealed record SolicitudDireccion(string Direccion, string Version);

public sealed record PaginaCreadaRespuesta(Guid Id);

public sealed record PaginaVersionRespuesta(string Version);
