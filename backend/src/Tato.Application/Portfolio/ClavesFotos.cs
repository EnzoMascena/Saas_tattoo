namespace Tato.Application.Portfolio;

/// <summary>
/// Claves de almacenamiento de las versiones de una foto: «estudios/{estudio}/fotos/{foto}/{tamaño}.webp».
/// Llevan el estudio para que sus archivos queden separados también en el almacenamiento (D-42).
/// </summary>
internal static class ClavesFotos
{
    public static string De(Guid estudioId, Guid fotoId, TamanioFoto tamanio) =>
        $"estudios/{estudioId}/fotos/{fotoId}/{tamanio.ToString().ToLowerInvariant()}.webp";

    public static IEnumerable<string> Todas(Guid estudioId, Guid fotoId) =>
        Enum.GetValues<TamanioFoto>().Select(tamanio => De(estudioId, fotoId, tamanio));
}
