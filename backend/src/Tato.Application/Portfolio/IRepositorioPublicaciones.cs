using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Acceso de escritura a las publicaciones. Los comandos cargan el agregado completo, con sus fotos, lo cambian
/// con sus métodos de dominio y lo guardan. Para leer sin cambiar nada está <see cref="IConsultasPublicaciones"/>.
/// </summary>
public interface IRepositorioPublicaciones
{
    /// <summary>
    /// Devuelve la publicación con sus fotos, lista para cambiar, o null si no existe o es de otro estudio.
    /// </summary>
    Task<Publicacion?> ObtenerAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Guarda una publicación nueva.
    /// </summary>
    Task AgregarAsync(Publicacion publicacion, CancellationToken cancellationToken);

    /// <summary>
    /// Guarda los cambios de una publicación obtenida con <see cref="ObtenerAsync"/> y devuelve su versión nueva.
    /// </summary>
    /// <param name="versionEsperada">
    /// La versión que tenía el panel al abrirla, en base64. Si alguien la modificó desde entonces, lanza
    /// ConflictoDeVersionException en lugar de pisar sus cambios (concurrencia optimista, doc 04).
    /// </param>
    Task<string> GuardarCambiosAsync(
        Publicacion publicacion,
        string versionEsperada,
        CancellationToken cancellationToken);
}
