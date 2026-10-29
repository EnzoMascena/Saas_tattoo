namespace Tato.Application.Portfolio;

/// <summary>
/// Claves de almacenamiento de las versiones de una foto. Llevan el estudio para que sus archivos queden separados
/// también en el almacenamiento (D-42):
/// <list type="bullet">
/// <item>de una publicación: «estudios/{estudio}/fotos/{foto}/{tamaño}.webp»;</item>
/// <item>de una página (R-16): «estudios/{estudio}/paginas/{foto}/{tamaño}.webp».</item>
/// </list>
/// </summary>
internal static class ClavesFotos
{
    public static string De(Guid estudioId, Guid fotoId, TamanioFoto tamanio) =>
        Clave(estudioId, "fotos", fotoId, tamanio);

    public static IEnumerable<string> Todas(Guid estudioId, Guid fotoId) =>
        Enum.GetValues<TamanioFoto>().Select(tamanio => De(estudioId, fotoId, tamanio));

    public static string DePagina(Guid estudioId, Guid fotoId, TamanioFoto tamanio) =>
        Clave(estudioId, "paginas", fotoId, tamanio);

    public static IEnumerable<string> TodasDePagina(Guid estudioId, Guid fotoId) =>
        Enum.GetValues<TamanioFoto>().Select(tamanio => DePagina(estudioId, fotoId, tamanio));

    private static string Clave(Guid estudioId, string carpeta, Guid fotoId, TamanioFoto tamanio) =>
        $"estudios/{estudioId}/{carpeta}/{fotoId}/{tamanio.ToString().ToLowerInvariant()}.webp";
}
