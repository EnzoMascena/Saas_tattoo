using Tato.Application.Comun;
using Tato.Application.Estudios;
using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Caso de uso: cambiar los datos de una publicación. Se puede en cualquier estado (ver Publicacion.Editar).
/// </summary>
public sealed class EditarPublicacion
{
    private readonly IRepositorioPublicaciones _repositorio;
    private readonly IReferenciasDelEstudio _referencias;
    private readonly IUsuarioActual _usuarioActual;
    private readonly TimeProvider _reloj;

    public EditarPublicacion(
        IRepositorioPublicaciones repositorio,
        IReferenciasDelEstudio referencias,
        IUsuarioActual usuarioActual,
        TimeProvider reloj)
    {
        _repositorio = repositorio;
        _referencias = referencias;
        _usuarioActual = usuarioActual;
        _reloj = reloj;
    }

    /// <summary>
    /// Aplica los datos nuevos y devuelve la versión nueva de la publicación.
    /// </summary>
    /// <param name="version">La versión que tenía el panel al abrirla (ver PublicacionDetalle.Version).</param>
    public async Task<string> EjecutarAsync(
        Guid id,
        DatosPublicacion datos,
        string version,
        CancellationToken cancellationToken = default)
    {
        var publicacion = await _repositorio.ObtenerAsync(id, cancellationToken)
            ?? throw new NoEncontradoException("La publicación no existe.");

        PermisosPublicaciones.ExigirEditar(_usuarioActual, publicacion);

        // Antes de editar: compara los Ids nuevos con los que tiene ahora la publicación (R-11).
        await ReferenciasPublicacion.VerificarAsync(datos, publicacion, _referencias, cancellationToken);

        publicacion.Editar(datos, _reloj.GetUtcNow());

        return await _repositorio.GuardarCambiosAsync(publicacion, version, cancellationToken);
    }
}
