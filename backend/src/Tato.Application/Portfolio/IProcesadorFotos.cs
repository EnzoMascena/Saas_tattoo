namespace Tato.Application.Portfolio;

/// <summary>
/// Puerto que convierte una foto subida desde el celular en sus versiones optimizadas (RN-BLG-05).
/// Lo implementa Infrastructure con una librería de imágenes; los casos de uso no saben cuál.
/// </summary>
public interface IProcesadorFotos
{
    /// <summary>
    /// Valida la foto y genera sus versiones: la gira según cómo se sacó, le quita los metadatos (como la
    /// ubicación GPS) y la guarda en WebP en cada <see cref="TamanioFoto"/>.
    /// </summary>
    /// <exception cref="FotoInvalidaException">Si no es una imagen válida o supera los límites.</exception>
    Task<FotoProcesada> ProcesarAsync(Stream original, CancellationToken cancellationToken);
}
