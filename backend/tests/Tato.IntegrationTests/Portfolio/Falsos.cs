using System.Collections.Concurrent;
using Tato.Application.Comun;
using Tato.Application.Estudios;
using Tato.Application.Portfolio;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// Reemplaza a la sesión: un usuario con los roles que elige cada test.
/// </summary>
internal sealed record UsuarioFalso(Guid? UsuarioId, bool EsDuenio, bool EsAdministrativo, Guid? TatuadorId = null) : IUsuarioActual
{
    public static UsuarioFalso Duenio() => new(Guid.NewGuid(), EsDuenio: true, EsAdministrativo: false);

    public static UsuarioFalso Tatuador() => new(Guid.NewGuid(), EsDuenio: false, EsAdministrativo: false);
}

/// <summary>
/// Reemplaza a la sesión: siempre el mismo estudio.
/// </summary>
internal sealed record EstudioFalso(Guid? EstudioId) : IEstudioActual;

/// <summary>
/// Un reloj que siempre da la misma hora, para que los tests no dependan de cuándo se corren.
/// </summary>
internal sealed class RelojFijo : TimeProvider
{
    private readonly DateTimeOffset _ahora;

    public RelojFijo(DateTimeOffset ahora)
    {
        _ahora = ahora;
    }

    public override DateTimeOffset GetUtcNow() => _ahora;
}

/// <summary>
/// Almacenamiento en memoria: deja ver qué archivos quedaron guardados después de cada caso de uso.
/// </summary>
internal sealed class AlmacenamientoEnMemoria : IAlmacenamientoArchivos
{
    public ConcurrentDictionary<string, byte[]> Archivos { get; } = new();

    public async Task GuardarAsync(string clave, Stream contenido, CancellationToken cancellationToken)
    {
        using var memoria = new MemoryStream();
        await contenido.CopyToAsync(memoria, cancellationToken);
        Archivos[clave] = memoria.ToArray();
    }

    public Task<Stream?> AbrirAsync(string clave, CancellationToken cancellationToken) =>
        Task.FromResult<Stream?>(Archivos.TryGetValue(clave, out var bytes) ? new MemoryStream(bytes) : null);

    public Task BorrarAsync(string clave, CancellationToken cancellationToken)
    {
        Archivos.TryRemove(clave, out _);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Las claves de los archivos de una foto.
    /// </summary>
    public IEnumerable<string> DeLaFoto(Guid fotoId) =>
        Archivos.Keys.Where(clave => clave.Contains(fotoId.ToString()));
}

/// <summary>
/// Reemplaza a NetVips: devuelve las tres versiones al instante. El procesador real tiene sus propios tests.
/// </summary>
internal sealed class ProcesadorFalso : IProcesadorFotos
{
    public const int Ancho = 800;
    public const int Alto = 600;

    public Task<FotoProcesada> ProcesarAsync(Stream original, CancellationToken cancellationToken) =>
        Task.FromResult(new FotoProcesada(
            Enum.GetValues<TamanioFoto>()
                .Select(tamanio => new VersionFoto(tamanio, [1, 2, 3], Ancho, Alto))
                .ToList()));
}

/// <summary>
/// Rechaza toda foto, como NetVips con un archivo que no es una imagen.
/// </summary>
internal sealed class ProcesadorQueRechaza : IProcesadorFotos
{
    public Task<FotoProcesada> ProcesarAsync(Stream original, CancellationToken cancellationToken) =>
        throw new FotoInvalidaException("El archivo no es una imagen.");
}
