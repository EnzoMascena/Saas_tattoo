namespace Tato.Application.Portfolio;

/// <summary>
/// Versiones que se generan de cada foto al subirla (RN-BLG-05). El sitio nunca sirve el original.
/// Las medidas de cada una son configuración, no código.
/// </summary>
public enum TamanioFoto
{
    /// <summary>
    /// Para listas y para el panel.
    /// </summary>
    Miniatura = 1,

    /// <summary>
    /// Para la grilla de la galería.
    /// </summary>
    Galeria = 2,

    /// <summary>
    /// Para la página de detalle de un trabajo.
    /// </summary>
    Detalle = 3,
}
