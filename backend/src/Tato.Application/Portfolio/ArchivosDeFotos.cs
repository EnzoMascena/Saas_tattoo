using Tato.Application.Comun;

namespace Tato.Application.Portfolio;

/// <summary>
/// Guardar y borrar los archivos de una foto. Lo comparten las fotos de las publicaciones y las de las páginas, que
/// solo cambian en la clave.
/// </summary>
internal static class ArchivosDeFotos
{
    /// <summary>
    /// Guarda cada versión de la foto con la clave que arma <paramref name="clave"/> para su tamaño.
    /// </summary>
    public static async Task GuardarAsync(
        IAlmacenamientoArchivos almacenamiento,
        FotoProcesada procesada,
        Func<TamanioFoto, string> clave,
        CancellationToken cancellationToken)
    {
        foreach (var versionFoto in procesada.Versiones)
        {
            using var contenido = new MemoryStream(versionFoto.Contenido, writable: false);
            await almacenamiento.GuardarAsync(clave(versionFoto.Tamanio), contenido, cancellationToken);
        }
    }

    /// <summary>
    /// Sin CancellationToken a propósito: la limpieza tiene que terminar aunque se haya cancelado el pedido.
    /// </summary>
    public static async Task BorrarAsync(IAlmacenamientoArchivos almacenamiento, IEnumerable<string> claves)
    {
        foreach (var clave in claves)
            await almacenamiento.BorrarAsync(clave, CancellationToken.None);
    }
}
