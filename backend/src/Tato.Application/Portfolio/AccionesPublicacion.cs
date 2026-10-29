using Tato.Application.Comun;
using Tato.Application.Estudios;
using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Casos de uso que cambian el estado de una publicación: confirmar el consentimiento, publicar y ocultar.
/// Los tres cargan la publicación, verifican el permiso (D-51), llaman al dominio y guardan con la versión que
/// tenía el panel, así que comparten <see cref="AplicarAsync"/>. Cada uno devuelve la versión nueva.
/// </summary>
public sealed class AccionesPublicacion
{
    private readonly IRepositorioPublicaciones _repositorio;
    private readonly IUsuarioActual _usuarioActual;
    private readonly TimeProvider _reloj;

    public AccionesPublicacion(
        IRepositorioPublicaciones repositorio,
        IUsuarioActual usuarioActual,
        TimeProvider reloj)
    {
        _repositorio = repositorio;
        _usuarioActual = usuarioActual;
        _reloj = reloj;
    }

    /// <summary>
    /// Registra que el cliente aceptó difundir las fotos (RN-BLG-03). Puede hacerlo quien puede editarla.
    /// </summary>
    public Task<string> ConfirmarConsentimientoAsync(
        Guid id,
        string version,
        CancellationToken cancellationToken = default) =>
        AplicarAsync(id, version, (publicacion, ahora) =>
        {
            PermisosPublicaciones.ExigirEditar(_usuarioActual, publicacion);

            var usuarioId = _usuarioActual.UsuarioId
                ?? throw new InvalidOperationException("No hay un usuario en la sesión.");

            publicacion.ConfirmarConsentimiento(usuarioId, ahora);
        }, cancellationToken);

    /// <summary>
    /// La muestra en el sitio público. Puede su artista o, en la página del estudio, un Dueño o un Administrativo
    /// (D-51).
    /// </summary>
    public Task<string> PublicarAsync(Guid id, string version, CancellationToken cancellationToken = default) =>
        AplicarAsync(id, version, (publicacion, ahora) =>
        {
            PermisosPublicaciones.ExigirPublicarUOcultar(_usuarioActual, publicacion);
            publicacion.Publicar(ahora);
        }, cancellationToken);

    /// <summary>
    /// La saca del sitio público sin borrarla (RN-BLG-06). Puede su artista o, en la página del estudio, un Dueño
    /// o un Administrativo (D-51).
    /// </summary>
    public Task<string> OcultarAsync(Guid id, string version, CancellationToken cancellationToken = default) =>
        AplicarAsync(id, version, (publicacion, ahora) =>
        {
            PermisosPublicaciones.ExigirPublicarUOcultar(_usuarioActual, publicacion);
            publicacion.Ocultar(ahora);
        }, cancellationToken);

    private async Task<string> AplicarAsync(
        Guid id,
        string version,
        Action<Publicacion, DateTimeOffset> accion,
        CancellationToken cancellationToken)
    {
        var publicacion = await _repositorio.ObtenerAsync(id, cancellationToken)
            ?? throw new NoEncontradoException("La publicación no existe.");

        accion(publicacion, _reloj.GetUtcNow());

        return await _repositorio.GuardarCambiosAsync(publicacion, version, cancellationToken);
    }
}
