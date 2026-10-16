namespace Tato.Application.Comun;

/// <summary>
/// Puerto para guardar archivos: fotos del blog hoy, comprobantes de seña en el MVP (doc 06, 6.1).
/// En desarrollo lo implementa el disco local; en producción, el disco del servidor o un almacenamiento de
/// objetos, según el hosting (P-18). Los casos de uso no saben cuál es.
/// </summary>
public interface IAlmacenamientoArchivos
{
    /// <summary>
    /// Guarda el contenido con esa clave. Si ya existe, lo reemplaza.
    /// </summary>
    /// <param name="clave">Ruta lógica del archivo, por ejemplo «estudios/{id}/fotos/{id}/detalle.webp».</param>
    Task GuardarAsync(string clave, Stream contenido, CancellationToken cancellationToken);

    /// <summary>
    /// Abre el archivo para leerlo, o devuelve null si no existe. Quien lo abre lo cierra.
    /// </summary>
    Task<Stream?> AbrirAsync(string clave, CancellationToken cancellationToken);

    /// <summary>
    /// Borra el archivo. Si no existe, no hace nada.
    /// </summary>
    Task BorrarAsync(string clave, CancellationToken cancellationToken);
}
