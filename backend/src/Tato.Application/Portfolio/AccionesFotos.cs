using Tato.Application.Comun;
using Tato.Application.Estudios;
using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Casos de uso sobre las fotos de una publicación: quitar, reordenar y elegir la portada. Puede hacerlos quien
/// puede editar la publicación (P-21). Cada uno guarda con la versión que tenía el panel y devuelve la nueva.
/// </summary>
public sealed class AccionesFotos
{
    private readonly IRepositorioPublicaciones _repositorio;
    private readonly IAlmacenamientoArchivos _almacenamiento;
    private readonly IUsuarioActual _usuarioActual;
    private readonly TimeProvider _reloj;

    public AccionesFotos(
        IRepositorioPublicaciones repositorio,
        IAlmacenamientoArchivos almacenamiento,
        IUsuarioActual usuarioActual,
        TimeProvider reloj)
    {
        _repositorio = repositorio;
        _almacenamiento = almacenamiento;
        _usuarioActual = usuarioActual;
        _reloj = reloj;
    }

    /// <summary>
    /// Quita la foto y borra sus archivos. Una publicación publicada no se puede quedar sin fotos (RN-BLG-04).
    /// </summary>
    public async Task<string> QuitarAsync(
        Guid publicacionId,
        Guid fotoId,
        string version,
        CancellationToken cancellationToken = default)
    {
        var publicacion = await ObtenerParaEditarAsync(publicacionId, cancellationToken);

        publicacion.QuitarFoto(fotoId, _reloj.GetUtcNow());

        // Primero la base: si falla, la foto sigue en la publicación y sus archivos siguen existiendo.
        var versionNueva = await _repositorio.GuardarCambiosAsync(publicacion, version, cancellationToken);

        // Después los archivos. Si esto falla, quedan archivos que nadie muestra, pero nada se rompe.
        foreach (var clave in ClavesFotos.Todas(publicacion.EstudioId, fotoId))
            await _almacenamiento.BorrarAsync(clave, CancellationToken.None);

        return versionNueva;
    }

    /// <summary>
    /// Ordena las fotos según la lista, que trae cada foto una sola vez. La primera queda como portada.
    /// </summary>
    public async Task<string> ReordenarAsync(
        Guid publicacionId,
        IReadOnlyList<Guid> fotoIds,
        string version,
        CancellationToken cancellationToken = default)
    {
        var publicacion = await ObtenerParaEditarAsync(publicacionId, cancellationToken);

        publicacion.Reordenar(fotoIds, _reloj.GetUtcNow());

        return await _repositorio.GuardarCambiosAsync(publicacion, version, cancellationToken);
    }

    /// <summary>
    /// Pone la foto como portada: pasa a ser la primera y las demás se corren un lugar.
    /// </summary>
    public async Task<string> ElegirPortadaAsync(
        Guid publicacionId,
        Guid fotoId,
        string version,
        CancellationToken cancellationToken = default)
    {
        var publicacion = await ObtenerParaEditarAsync(publicacionId, cancellationToken);

        publicacion.ElegirPortada(fotoId, _reloj.GetUtcNow());

        return await _repositorio.GuardarCambiosAsync(publicacion, version, cancellationToken);
    }

    private async Task<Publicacion> ObtenerParaEditarAsync(Guid publicacionId, CancellationToken cancellationToken)
    {
        var publicacion = await _repositorio.ObtenerAsync(publicacionId, cancellationToken)
            ?? throw new NoEncontradoException("La publicación no existe.");

        PermisosPublicaciones.ExigirEditar(_usuarioActual, publicacion);

        return publicacion;
    }
}
