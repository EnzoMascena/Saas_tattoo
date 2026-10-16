using System.Text;
using Microsoft.Extensions.Options;
using Tato.Infrastructure.Comun;

namespace Tato.IntegrationTests.Comun;

/// <summary>
/// Almacenamiento en disco contra una carpeta temporal de verdad, que se borra al terminar cada test.
/// </summary>
public sealed class AlmacenamientoEnDiscoTests : IDisposable
{
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), $"tato-tests-{Guid.NewGuid():N}");
    private readonly AlmacenamientoEnDisco _almacenamiento;

    public AlmacenamientoEnDiscoTests()
    {
        _almacenamiento = new AlmacenamientoEnDisco(
            Options.Create(new OpcionesAlmacenamientoDisco { CarpetaRaiz = _carpeta }));
    }

    public void Dispose()
    {
        if (Directory.Exists(_carpeta))
            Directory.Delete(_carpeta, recursive: true);
    }

    [Fact]
    public async Task Guardar_YAbrir_DevuelveElMismoContenido()
    {
        // Act
        await GuardarTextoAsync("estudios/a/fotos/b/detalle.webp", "contenido");

        // Assert
        Assert.Equal("contenido", await LeerTextoAsync("estudios/a/fotos/b/detalle.webp"));
    }

    [Fact]
    public async Task Guardar_DosVeces_ReemplazaElContenido()
    {
        await GuardarTextoAsync("archivo.txt", "primero");
        await GuardarTextoAsync("archivo.txt", "segundo");

        Assert.Equal("segundo", await LeerTextoAsync("archivo.txt"));
    }

    [Fact]
    public async Task Guardar_NoDejaArchivosTemporales()
    {
        await GuardarTextoAsync("carpeta/archivo.txt", "contenido");

        var archivos = Directory.GetFiles(Path.Combine(_carpeta, "carpeta"));
        Assert.Equal(Path.Combine(_carpeta, "carpeta", "archivo.txt"), Assert.Single(archivos));
    }

    [Fact]
    public async Task Abrir_ArchivoQueNoExiste_DevuelveNull()
    {
        Assert.Null(await _almacenamiento.AbrirAsync("no/existe.webp", CancellationToken.None));
    }

    [Fact]
    public async Task Borrar_ArchivoExistente_YaNoSePuedeAbrir()
    {
        // Arrange
        await GuardarTextoAsync("archivo.txt", "contenido");

        // Act
        await _almacenamiento.BorrarAsync("archivo.txt", CancellationToken.None);

        // Assert
        Assert.Null(await _almacenamiento.AbrirAsync("archivo.txt", CancellationToken.None));
    }

    [Fact]
    public async Task Borrar_ArchivoQueNoExiste_NoLanzaExcepcion()
    {
        await _almacenamiento.BorrarAsync("no/existe.webp", CancellationToken.None);
    }

    [Theory]
    [InlineData("../afuera.txt")]
    [InlineData("estudios/../../afuera.txt")]
    [InlineData("/etc/passwd")]
    [InlineData("")]
    public async Task Guardar_ConClaveQueSaleDeLaCarpeta_LanzaArgumentException(string clave)
    {
        // Act & Assert: ni siquiera se crea el archivo.
        await Assert.ThrowsAsync<ArgumentException>(() => GuardarTextoAsync(clave, "contenido"));
        Assert.False(File.Exists(Path.Combine(Path.GetTempPath(), "afuera.txt")));
    }

    [Fact]
    public void Crear_SinCarpetaConfigurada_LanzaInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new AlmacenamientoEnDisco(Options.Create(new OpcionesAlmacenamientoDisco())));
    }

    private async Task GuardarTextoAsync(string clave, string texto)
    {
        using var contenido = new MemoryStream(Encoding.UTF8.GetBytes(texto));
        await _almacenamiento.GuardarAsync(clave, contenido, CancellationToken.None);
    }

    private async Task<string?> LeerTextoAsync(string clave)
    {
        await using var archivo = await _almacenamiento.AbrirAsync(clave, CancellationToken.None);

        if (archivo is null)
            return null;

        using var lector = new StreamReader(archivo, Encoding.UTF8);
        return await lector.ReadToEndAsync();
    }
}
