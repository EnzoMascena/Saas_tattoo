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
    /// Guarda los cambios de una publicación que ya se obtuvo con <see cref="ObtenerAsync"/>.
    /// Si alguien la modificó en el medio, lanza una excepción de concurrencia en lugar de pisar los cambios.
    /// </summary>
    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}