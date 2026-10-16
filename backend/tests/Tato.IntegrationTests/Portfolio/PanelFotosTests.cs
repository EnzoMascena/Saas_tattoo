using Tato.Application.Comun;
using Tato.Domain.Comun;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// Casos de uso de fotos contra SQL Server: que la base y los archivos queden siempre de acuerdo, aun cuando
/// algo falla en el medio (RN-BLG-04, RN-BLG-05).
/// </summary>
public sealed class PanelFotosTests : IClassFixture<BaseDeDatosFixture>
{
    private readonly BaseDeDatosFixture _baseDeDatos;

    public PanelFotosTests(BaseDeDatosFixture baseDeDatos)
    {
        _baseDeDatos = baseDeDatos;
    }

    [Fact]
    public async Task SubirFoto_GuardaLasTresVersionesYLaAgregaAlFinal()
    {
        // Arrange
        var panel = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var id = await panel.CrearPublicacionAsync("Con fotos");
        var version = (await panel.ObtenerAsync(id)).Version;

        // Act
        var subida = await panel.SubirFotoAsync(id, version);

        // Assert: en la base.
        var detalle = await panel.ObtenerAsync(id);
        var foto = Assert.Single(detalle.Fotos);
        Assert.Equal(subida.Foto.Id, foto.Id);
        Assert.Equal(0, foto.Orden);
        Assert.Equal(ProcesadorFalso.Ancho, foto.Ancho);
        Assert.Equal(ProcesadorFalso.Alto, foto.Alto);
        Assert.Equal(subida.Version, detalle.Version);

        // Assert: en el almacenamiento, una por tamaño y dentro de la carpeta del estudio (D-42).
        var claves = panel.Almacenamiento.DeLaFoto(foto.Id).ToList();
        Assert.Equal(3, claves.Count);
        Assert.All(claves, clave => Assert.StartsWith($"estudios/{panel.EstudioId}/", clave));
    }

    [Fact]
    public async Task SubirFoto_ConUnaVersionVieja_BorraSusArchivosYNoLaAgrega()
    {
        // Arrange: la primera foto se sube bien.
        var panel = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var id = await panel.CrearPublicacionAsync("Con fotos");
        var versionAlAbrir = (await panel.ObtenerAsync(id)).Version;
        await panel.SubirFotoAsync(id, versionAlAbrir);

        // Act & Assert: la segunda llega con la versión vieja. Sus archivos ya se habían guardado.
        await Assert.ThrowsAsync<ConflictoDeVersionException>(() => panel.SubirFotoAsync(id, versionAlAbrir));

        // Assert: queda solo la primera, en la base y en el almacenamiento. No hay archivos huérfanos.
        Assert.Single((await panel.ObtenerAsync(id)).Fotos);
        Assert.Equal(3, panel.Almacenamiento.Archivos.Count);
    }

    [Fact]
    public async Task SubirFoto_PasadoElMaximoDelEstudio_LanzaLaReglaYNoGuardaArchivos()
    {
        // Arrange: el estudio permite una sola foto por publicación.
        var panel = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio(), maxFotos: 1);
        var id = await panel.CrearPublicacionAsync("Una sola foto");
        var primera = await panel.SubirFotoAsync(id, (await panel.ObtenerAsync(id)).Version);

        // Act & Assert
        var error = await Assert.ThrowsAsync<ReglaDeNegocioException>(
            () => panel.SubirFotoAsync(id, primera.Version));
        Assert.Equal("RN-BLG-04", error.Regla);

        // Assert: solo los archivos de la primera.
        Assert.Equal(3, panel.Almacenamiento.Archivos.Count);
    }

    [Fact]
    public async Task QuitarFoto_BorraLaFotoYSusArchivos()
    {
        // Arrange: dos fotos.
        var panel = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var id = await panel.CrearPublicacionAsync("Con dos fotos");
        var primera = await panel.SubirFotoAsync(id, (await panel.ObtenerAsync(id)).Version);
        var segunda = await panel.SubirFotoAsync(id, primera.Version);

        // Act
        await panel.QuitarFotoAsync(id, primera.Foto.Id, segunda.Version);

        // Assert: queda la segunda, ahora en el lugar 0, y la primera ya no tiene archivos.
        var foto = Assert.Single((await panel.ObtenerAsync(id)).Fotos);
        Assert.Equal(segunda.Foto.Id, foto.Id);
        Assert.Equal(0, foto.Orden);
        Assert.Empty(panel.Almacenamiento.DeLaFoto(primera.Foto.Id));
        Assert.Equal(3, panel.Almacenamiento.DeLaFoto(segunda.Foto.Id).Count());
    }
}
