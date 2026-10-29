using Tato.Application.Portfolio;
using Tato.Domain.Portfolio;
using Tato.Infrastructure.Portfolio;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// Qué fotos salen en el sitio público, sin sesión: solo las de publicaciones Publicadas de páginas visibles y las
/// de páginas visibles (RN-BLG-01, R-16). Las demás no se pueden pedir ni sabiendo su Id.
/// </summary>
public sealed class FotosDelSitioTests : IClassFixture<BaseDeDatosFixture>
{
    private readonly BaseDeDatosFixture _baseDeDatos;

    public FotosDelSitioTests(BaseDeDatosFixture baseDeDatos)
    {
        _baseDeDatos = baseDeDatos;
    }

    [Fact]
    public async Task DeUnaPublicadaDeUnaPaginaVisible_SeVe()
    {
        // Arrange: una en la página del estudio y otra en la de una artista.
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var artista = duenio.Como(await duenio.CrearArtistaAsync());
        await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        await artista.CrearPaginaVisibleAsync(delEstudio: false, NombreUnico("Ink Mortal"));
        var delEstudio = await duenio.PublicarNuevaAsync(new DatosPublicacion("Del estudio"));
        var deLaArtista = await artista.PublicarNuevaAsync(
            new DatosPublicacion("De la artista", TatuadorId: artista.Usuario.TatuadorId));

        // Act & Assert
        Assert.True(await SeVeAsync(duenio, await PortadaAsync(duenio, delEstudio)));
        Assert.True(await SeVeAsync(duenio, await PortadaAsync(artista, deLaArtista)));
    }

    [Fact]
    public async Task DeUnBorradorODeUnaOculta_NoSeVe()
    {
        // Arrange
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        var borrador = await duenio.CrearPublicacionAsync("Borrador");
        await duenio.SubirFotoAsync(borrador, (await duenio.ObtenerAsync(borrador)).Version);
        var oculta = await duenio.PublicarNuevaAsync(new DatosPublicacion("Oculta"));
        await duenio.OcultarAsync(oculta, (await duenio.ObtenerAsync(oculta)).Version);

        // Act & Assert
        Assert.False(await SeVeAsync(duenio, await PortadaAsync(duenio, borrador)));
        Assert.False(await SeVeAsync(duenio, await PortadaAsync(duenio, oculta)));
    }

    [Fact]
    public async Task DeUnaPublicadaDeUnaPaginaOcultaOQueNoExiste_NoSeVe()
    {
        // Arrange: la artista oculta su página, y el estudio nunca creó la suya.
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var artista = duenio.Como(await duenio.CrearArtistaAsync());
        var (idDeLaArtista, _) = await artista.CrearPaginaVisibleAsync(delEstudio: false, NombreUnico("Ink Mortal"));
        var deLaArtista = await artista.PublicarNuevaAsync(
            new DatosPublicacion("De la artista", TatuadorId: artista.Usuario.TatuadorId));
        await artista.OcultarPaginaAsync(idDeLaArtista, (await artista.ObtenerPaginaAsync(idDeLaArtista)).Version);
        var delEstudio = await duenio.PublicarNuevaAsync(new DatosPublicacion("Del estudio, sin página"));

        // Act & Assert
        Assert.False(await SeVeAsync(duenio, await PortadaAsync(artista, deLaArtista)));
        Assert.False(await SeVeAsync(duenio, await PortadaAsync(duenio, delEstudio)));
    }

    [Fact]
    public async Task DeUnaPagina_SeVeSoloMientrasLaPaginaEstaVisible()
    {
        // Arrange: la del estudio, con sus dos fotos.
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var (id, _) = await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        var encabezado = await duenio.SubirFotoPaginaAsync(
            id, LugarFotoPagina.Encabezado, (await duenio.ObtenerPaginaAsync(id)).Version);
        var sobre = await duenio.SubirFotoPaginaAsync(id, LugarFotoPagina.Sobre, encabezado.Version);

        // Act & Assert: visible, se ven las dos.
        Assert.True(await SeVeAsync(duenio, encabezado.Foto.Id));
        Assert.True(await SeVeAsync(duenio, sobre.Foto.Id));

        // Act & Assert: oculta, ninguna.
        await duenio.OcultarPaginaAsync(id, sobre.Version);
        Assert.False(await SeVeAsync(duenio, encabezado.Foto.Id));
        Assert.False(await SeVeAsync(duenio, sobre.Foto.Id));
    }

    [Fact]
    public async Task QueNoExiste_NoSeVe()
    {
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());

        Assert.False(await SeVeAsync(duenio, Guid.NewGuid()));
    }

    /// <summary>
    /// Pide la foto como el sitio: sin estudio en la sesión, contra los archivos que guardó el panel.
    /// </summary>
    private async Task<bool> SeVeAsync(Panel panel, Guid fotoId)
    {
        await using var db = _baseDeDatos.CrearContexto(estudioId: null);
        var leer = new LeerFotoDelSitio(new ConsultasDelSitio(db), panel.Almacenamiento);
        await using var contenido = await leer.EjecutarAsync(fotoId, TamanioFoto.Galeria);

        return contenido is not null;
    }

    private static async Task<Guid> PortadaAsync(Panel panel, Guid publicacionId) =>
        (await panel.ObtenerAsync(publicacionId)).Fotos[0].Id;

    /// <summary>
    /// Todos los tests de la clase comparten la base: cada página necesita un nombre, y una dirección, únicos.
    /// </summary>
    private static string NombreUnico(string nombre) => $"{nombre} {Guid.NewGuid():N}";
}
