namespace Tato.Application.Portfolio;

/// <summary>
/// Una versión optimizada de una foto, ya en WebP y sin metadatos.
/// </summary>
/// <param name="Tamanio">Qué versión es.</param>
/// <param name="Contenido">Los bytes del archivo WebP.</param>
/// <param name="Ancho">Ancho en píxeles.</param>
/// <param name="Alto">Alto en píxeles.</param>
public sealed record VersionFoto(TamanioFoto Tamanio, byte[] Contenido, int Ancho, int Alto);
