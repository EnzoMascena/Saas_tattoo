using Tato.Application.Estudios;
using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Caso de uso: crear una publicación en Borrador desde el panel. El estudio y el usuario salen de la sesión,
/// nunca de un dato que mande el navegador (D-42).
/// </summary>
public sealed class CrearPublicacion
{
    private readonly IRepositorioPublicaciones _repositorio;
    private readonly IEstudioActual _estudioActual;
    private readonly IUsuarioActual _usuarioActual;
    private readonly TimeProvider _reloj;

    public CrearPublicacion(
        IRepositorioPublicaciones repositorio,
        IEstudioActual estudioActual,
        IUsuarioActual usuarioActual,
        TimeProvider reloj)
    {
        _repositorio = repositorio;
        _estudioActual = estudioActual;
        _usuarioActual = usuarioActual;
        _reloj = reloj;
    }

    /// <summary>
    /// Crea la publicación y devuelve su Id. Si un dato no sirve, el dominio lanza ArgumentException (400).
    /// </summary>
    public async Task<Guid> EjecutarAsync(DatosPublicacion datos, CancellationToken cancellationToken = default)
    {
        // Sin sesión no se llega acá: el endpoint exige estar logueado. Si pasa, es un error de programación.
        var estudioId = _estudioActual.EstudioId
            ?? throw new InvalidOperationException("No hay un estudio en la sesión.");

        var usuarioId = _usuarioActual.UsuarioId
            ?? throw new InvalidOperationException("No hay un usuario en la sesión.");

        var publicacion = Publicacion.Crear(estudioId, usuarioId, datos, _reloj.GetUtcNow());

        await _repositorio.AgregarAsync(publicacion, cancellationToken);

        return publicacion.Id;
    }
}
