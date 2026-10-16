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
    private readonly IReferenciasDelEstudio _referencias;
    private readonly IEstudioActual _estudioActual;
    private readonly IUsuarioActual _usuarioActual;
    private readonly TimeProvider _reloj;

    public CrearPublicacion(
        IRepositorioPublicaciones repositorio,
        IReferenciasDelEstudio referencias,
        IEstudioActual estudioActual,
        IUsuarioActual usuarioActual,
        TimeProvider reloj)
    {
        _repositorio = repositorio;
        _referencias = referencias;
        _estudioActual = estudioActual;
        _usuarioActual = usuarioActual;
        _reloj = reloj;
    }

    /// <summary>
    /// Crea la publicación y devuelve su Id. Un dato mal formado da ArgumentException (400); un estilo o una
    /// zona desactivados dan ReglaDeNegocioException (422).
    /// </summary>
    public async Task<Guid> EjecutarAsync(DatosPublicacion datos, CancellationToken cancellationToken = default)
    {
        // Sin sesión no se llega acá: el endpoint exige estar logueado. Si pasa, es un error de programación.
        var estudioId = _estudioActual.EstudioId
            ?? throw new InvalidOperationException("No hay un estudio en la sesión.");

        var usuarioId = _usuarioActual.UsuarioId
            ?? throw new InvalidOperationException("No hay un usuario en la sesión.");

        // Primero el dominio: valida el formato sin ir a la base.
        var publicacion = Publicacion.Crear(estudioId, usuarioId, datos, _reloj.GetUtcNow());

        // Después, que lo elegido exista y se pueda usar.
        await ReferenciasPublicacion.VerificarAsync(datos, actual: null, _referencias, cancellationToken);

        await _repositorio.AgregarAsync(publicacion, cancellationToken);

        return publicacion.Id;
    }
}
