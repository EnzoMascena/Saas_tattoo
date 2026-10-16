namespace Tato.Domain.Portfolio;

/// <summary>
/// En qué pestaña de la página pública aparece una publicación (D-50). Se guarda como número: los valores no
/// se cambian.
/// </summary>
public enum TipoPublicacion
{
    /// <summary>
    /// Un tatuaje terminado: aparece en la grilla de la pestaña Trabajos.
    /// </summary>
    Trabajo = 1,

    /// <summary>
    /// Una publicación con más texto: aparece en la pestaña Blog.
    /// </summary>
    Blog = 2,
}
