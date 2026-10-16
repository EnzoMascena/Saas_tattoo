namespace Tato.Infrastructure.Comun;

/// <summary>
/// Configuración del almacenamiento en disco (patrón Options, doc 06, 5). Se carga de la sección
/// «Almacenamiento:Disco» de appsettings: la carpeta no se escribe en el código.
/// </summary>
public sealed class OpcionesAlmacenamientoDisco
{
    public const string Seccion = "Almacenamiento:Disco";

    /// <summary>
    /// Carpeta donde se guardan los archivos. Puede ser relativa a la carpeta de la Api o absoluta.
    /// </summary>
    public string CarpetaRaiz { get; set; } = string.Empty;
}
