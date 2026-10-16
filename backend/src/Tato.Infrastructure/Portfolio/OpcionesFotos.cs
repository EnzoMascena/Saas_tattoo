namespace Tato.Infrastructure.Portfolio;

/// <summary>
/// Configuración del procesamiento de fotos (patrón Options, doc 06, 5). Los valores de acá son los que se
/// usan si appsettings no dice otra cosa, en la sección «Fotos».
/// </summary>
public sealed class OpcionesFotos
{
    public const string Seccion = "Fotos";

    /// <summary>
    /// Peso máximo del archivo subido. 20 MB alcanza para cualquier foto de celular.
    /// </summary>
    public long MaxBytes { get; set; } = 20 * 1024 * 1024;

    /// <summary>
    /// Máximo de píxeles (ancho por alto). 50 millones cubre celulares de 48 MP y frena las imágenes que pesan
    /// poco pero ocupan gigas de memoria al abrirlas.
    /// </summary>
    public long MaxPixeles { get; set; } = 50_000_000;

    /// <summary>
    /// Lado mayor, en píxeles, de cada versión (RN-BLG-05). Una foto más chica no se agranda.
    /// </summary>
    public int LadoMiniatura { get; set; } = 400;

    public int LadoGaleria { get; set; } = 1080;

    public int LadoDetalle { get; set; } = 2048;

    /// <summary>
    /// Calidad de WebP, de 1 a 100. 80 se ve igual que la original y pesa mucho menos.
    /// </summary>
    public int CalidadWebp { get; set; } = 80;
}
