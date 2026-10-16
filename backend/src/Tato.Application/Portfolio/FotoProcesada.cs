namespace Tato.Application.Portfolio;

/// <summary>
/// Resultado de procesar una foto subida: una versión por cada <see cref="TamanioFoto"/>.
/// </summary>
public sealed record FotoProcesada(IReadOnlyList<VersionFoto> Versiones)
{
    /// <summary>
    /// La versión de detalle, la más grande. Su ancho y su alto son los que se guardan en la Foto del dominio.
    /// </summary>
    public VersionFoto Detalle => Versiones.Single(v => v.Tamanio == TamanioFoto.Detalle);
}
