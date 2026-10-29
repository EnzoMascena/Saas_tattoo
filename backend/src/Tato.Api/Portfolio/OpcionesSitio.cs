namespace Tato.Api.Portfolio;

/// <summary>
/// Configuración de los endpoints del sitio público (patrón Options, D-34), en la sección «Sitio» de appsettings.
/// Los tiempos de caché dicen cuánto puede seguir viéndose algo después de ocultarlo (D-57).
/// </summary>
public sealed class OpcionesSitio
{
    public const string Seccion = "Sitio";

    /// <summary>
    /// Cuánto puede guardar el navegador las páginas y las publicaciones. Cloudflare no cachea JSON por defecto.
    /// </summary>
    public int SegundosCacheDatos { get; set; } = 60;

    /// <summary>
    /// Cuánto pueden guardar Cloudflare y el navegador cada foto. Es lo que una foto oculta puede seguir viéndose por
    /// link directo.
    /// </summary>
    public int SegundosCacheFotos { get; set; } = 3600;

    /// <summary>
    /// Hasta cuántos caracteres del texto van en la lista del Blog.
    /// </summary>
    public int LargoExtracto { get; set; } = 200;
}
