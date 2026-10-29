using Tato.Application.Comun;
using Tato.Domain.Portfolio;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// Permisos por página contra SQL Server (D-51): la página de un artista la gestiona solo ese artista, ni siquiera
/// un Dueño; la del estudio sigue los roles del doc 07, sección 5.
/// </summary>
public sealed class PanelPermisosPorPaginaTests : IClassFixture<BaseDeDatosFixture>
{
    private readonly BaseDeDatosFixture _baseDeDatos;

    public PanelPermisosPorPaginaTests(BaseDeDatosFixture baseDeDatos)
    {
        _baseDeDatos = baseDeDatos;
    }

    [Fact]
    public async Task Crear_UnArtistaEnSuPropiaPagina_Puede()
    {
        // Arrange
        var (_, belen, _) = await EstudioConDosArtistasAsync();

        // Act
        var id = await belen.CrearPublicacionAsync(EnLaPaginaDe(belen, "Rosa en el antebrazo"));

        // Assert
        Assert.Equal(belen.Usuario.TatuadorId, (await belen.ObtenerAsync(id)).TatuadorId);
    }

    [Fact]
    public async Task Crear_EnLaPaginaDeOtroArtista_LanzaAccesoDenegado()
    {
        // Arrange
        var (_, belen, emma) = await EstudioConDosArtistasAsync();

        // Act & Assert
        await Assert.ThrowsAsync<AccesoDenegadoException>(
            () => belen.CrearPublicacionAsync(EnLaPaginaDe(emma, "Intento")));
    }

    [Fact]
    public async Task Editar_UnDuenioQueNoEsElArtista_LanzaAccesoDenegado()
    {
        // Arrange: la publicación es de la página de Emma; quien edita es un Dueño sin página propia.
        var (duenio, _, emma) = await EstudioConDosArtistasAsync();
        var id = await emma.CrearPublicacionAsync(EnLaPaginaDe(emma, "De Emma"));
        var version = (await emma.ObtenerAsync(id)).Version;

        // Act & Assert
        await Assert.ThrowsAsync<AccesoDenegadoException>(
            () => duenio.EditarAsync(id, EnLaPaginaDe(emma, "Intento"), version));
    }

    [Fact]
    public async Task Editar_OtroArtistaAunqueSeaDuenio_LanzaAccesoDenegado()
    {
        // Arrange: Belén también es Dueña, pero la página es de Emma.
        var (_, belen, emma) = await EstudioConDosArtistasAsync();
        var id = await emma.CrearPublicacionAsync(EnLaPaginaDe(emma, "De Emma"));
        var version = (await emma.ObtenerAsync(id)).Version;

        // Act & Assert
        await Assert.ThrowsAsync<AccesoDenegadoException>(
            () => belen.EditarAsync(id, EnLaPaginaDe(emma, "Intento"), version));
    }

    [Fact]
    public async Task Publicar_UnArtistaEnSuPagina_Puede()
    {
        // Arrange: con una foto y el consentimiento, que exigen RN-BLG-03 y RN-BLG-04.
        var (_, belen, _) = await EstudioConDosArtistasAsync();
        var id = await belen.CrearPublicacionAsync(EnLaPaginaDe(belen, "Rosa en el antebrazo"));
        var subida = await belen.SubirFotoAsync(id, (await belen.ObtenerAsync(id)).Version);
        var version = await belen.ConfirmarConsentimientoAsync(id, subida.Version);

        // Act
        await belen.PublicarAsync(id, version);

        // Assert
        Assert.Equal(EstadoPublicacion.Publicada, (await belen.ObtenerAsync(id)).Estado);
    }

    [Fact]
    public async Task Publicar_UnDuenioEnLaPaginaDeUnArtista_LanzaAccesoDenegado()
    {
        // Arrange
        var (duenio, _, emma) = await EstudioConDosArtistasAsync();
        var id = await emma.CrearPublicacionAsync(EnLaPaginaDe(emma, "De Emma"));
        var version = (await emma.ObtenerAsync(id)).Version;

        // Act & Assert: el permiso se controla antes que las reglas del dominio (fotos y consentimiento).
        await Assert.ThrowsAsync<AccesoDenegadoException>(() => duenio.PublicarAsync(id, version));
    }

    [Fact]
    public async Task Editar_MoverALaPaginaDeOtroArtista_LanzaAccesoDenegado()
    {
        // Arrange: Belén crea en la página del estudio, que como Dueña puede editar.
        var (_, belen, emma) = await EstudioConDosArtistasAsync();
        var id = await belen.CrearPublicacionAsync("Del estudio");
        var version = (await belen.ObtenerAsync(id)).Version;

        // Act & Assert: pero no puede mandarla a la página de Emma.
        await Assert.ThrowsAsync<AccesoDenegadoException>(
            () => belen.EditarAsync(id, EnLaPaginaDe(emma, "Del estudio"), version));
    }

    [Fact]
    public async Task Editar_MoverASuPropiaPagina_Puede()
    {
        // Arrange
        var (_, belen, _) = await EstudioConDosArtistasAsync();
        var id = await belen.CrearPublicacionAsync("Del estudio");
        var version = (await belen.ObtenerAsync(id)).Version;

        // Act
        await belen.EditarAsync(id, EnLaPaginaDe(belen, "Ahora es mía"), version);

        // Assert
        Assert.Equal(belen.Usuario.TatuadorId, (await belen.ObtenerAsync(id)).TatuadorId);
    }

    /// <summary>
    /// Un estudio con un Dueño sin página propia y dos artistas que también son Dueños, como el estudio piloto.
    /// </summary>
    private async Task<(Panel Duenio, Panel Belen, Panel Emma)> EstudioConDosArtistasAsync()
    {
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var belen = duenio.Como(await duenio.CrearArtistaAsync());
        var emma = duenio.Como(await duenio.CrearArtistaAsync());
        return (duenio, belen, emma);
    }

    private static DatosPublicacion EnLaPaginaDe(Panel artista, string titulo) =>
        new(titulo, TatuadorId: artista.Usuario.TatuadorId);
}
