namespace Tato.Domain.Portfolio;

/// <summary>
/// Estado de una publicación del blog (doc 07, sección 5). Se guarda como número: los valores no se cambian.
/// <para>
/// Es un enum y no el patrón State: todas las transiciones están permitidas y las dos reglas que hay
/// (RN-BLG-03 y RN-BLG-04) se controlan al publicar. Es el mismo criterio que el doc 06 aplica al Tatuaje.
/// </para>
/// </summary>
public enum EstadoPublicacion
{
    /// <summary>
    /// Recién creada o en preparación. No se ve en el sitio público.
    /// </summary>
    Borrador = 1,

    /// <summary>
    /// Visible en el sitio público (RN-BLG-01).
    /// </summary>
    Publicada = 2,

    /// <summary>
    /// Sacada del sitio sin borrarla (RN-BLG-06). Se puede volver a publicar.
    /// </summary>
    Oculta = 3,
}
