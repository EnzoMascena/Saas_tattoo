using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Acceso de escritura a las páginas públicas. Para leer sin cambiar nada está <see cref="IConsultasPaginas"/>.
/// </summary>
public interface IRepositorioPaginas
{
    /// <summary>
    /// Devuelve la página lista para cambiar, o null si no existe o es de otro estudio.
    /// </summary>
    Task<PaginaPublica?> ObtenerAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Si ya existe la página del estudio de la sesión (con <paramref name="tatuadorId"/> null) o la de ese artista.
    /// </summary>
    Task<bool> ExisteAsync(Guid? tatuadorId, CancellationToken cancellationToken);

    /// <summary>
    /// Si alguna página de la plataforma, de cualquier estudio, ya usa esa dirección (RN-BLG-07).
    /// </summary>
    Task<bool> DireccionEnUsoAsync(DireccionPublica direccion, CancellationToken cancellationToken);

    /// <summary>
    /// Guarda una página nueva.
    /// </summary>
    Task AgregarAsync(PaginaPublica pagina, CancellationToken cancellationToken);

    /// <summary>
    /// Guarda los cambios de una página obtenida con <see cref="ObtenerAsync"/> y devuelve su versión nueva.
    /// </summary>
    /// <param name="versionEsperada">
    /// La versión que tenía el panel al abrirla, en base64. Si alguien la modificó desde entonces, lanza
    /// ConflictoDeVersionException en lugar de pisar sus cambios.
    /// </param>
    Task<string> GuardarCambiosAsync(PaginaPublica pagina, string versionEsperada, CancellationToken cancellationToken);
}
