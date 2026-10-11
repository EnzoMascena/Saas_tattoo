using Microsoft.Extensions.Options;
using Tato.Application.Comun;

namespace Tato.Infrastructure.Comun;

/// <summary>
/// Adaptador de <see cref="IAlmacenamientoArchivos"/> que guarda en una carpeta del disco. Sirve para desarrollo
/// y para un servidor con disco propio; con un almacenamiento de objetos se escribe otro adaptador (doc 06, 6.1).
/// </summary>
internal sealed class AlmacenamientoEnDisco : IAlmacenamientoArchivos
{
    private const int TamanioBuffer = 81_920;

    private readonly string _raiz;

    public AlmacenamientoEnDisco(IOptions<OpcionesAlmacenamientoDisco> opciones)
    {
        var carpeta = opciones.Value.CarpetaRaiz;

        // Si falta, mejor fallar al arrancar que al subir la primera foto.
        if (string.IsNullOrWhiteSpace(carpeta))
            throw new InvalidOperationException(
                $"Falta la configuración '{OpcionesAlmacenamientoDisco.Seccion}:CarpetaRaiz'.");

        _raiz = Path.GetFullPath(carpeta);
    }

    public async Task GuardarAsync(string clave, Stream contenido, CancellationToken cancellationToken)
    {
        var ruta = RutaDe(clave);
        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);

        // Se escribe en un archivo temporal y después se renombra: quien lea nunca ve un archivo a medio escribir,
        // y si algo falla no queda un archivo roto con el nombre bueno.
        var temporal = $"{ruta}.{Guid.NewGuid():N}.tmp";

        try
        {
            await using (var archivo = new FileStream(
                temporal, FileMode.CreateNew, FileAccess.Write, FileShare.None, TamanioBuffer, useAsync: true))
            {
                await contenido.CopyToAsync(archivo, cancellationToken);
            }

            File.Move(temporal, ruta, overwrite: true);
        }
        catch
        {
            File.Delete(temporal);
            throw;
        }
    }

    public Task<Stream?> AbrirAsync(string clave, CancellationToken cancellationToken)
    {
        var ruta = RutaDe(clave);

        try
        {
            Stream archivo = new FileStream(
                ruta, FileMode.Open, FileAccess.Read, FileShare.Read, TamanioBuffer, useAsync: true);
            return Task.FromResult<Stream?>(archivo);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            return Task.FromResult<Stream?>(null);
        }
    }

    public Task BorrarAsync(string clave, CancellationToken cancellationToken)
    {
        var ruta = RutaDe(clave);

        // File.Delete no falla si falta el archivo, pero sí si falta la carpeta: por eso se pregunta antes.
        if (File.Exists(ruta))
            File.Delete(ruta);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Convierte la clave en una ruta dentro de la carpeta raíz. Rechaza las claves que intentan salir de ella
    /// («../../etc/passwd») o que son rutas absolutas: aunque las claves las arma el sistema, un error ahí no
    /// puede terminar escribiendo en cualquier lugar del servidor.
    /// </summary>
    private string RutaDe(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave) || Path.IsPathRooted(clave) || clave.Contains('\\'))
            throw new ArgumentException("La clave del archivo no es válida.", nameof(clave));

        var ruta = Path.GetFullPath(Path.Combine(_raiz, clave));

        if (!ruta.StartsWith(_raiz + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new ArgumentException("La clave del archivo apunta fuera de la carpeta raíz.", nameof(clave));

        return ruta;
    }
}
