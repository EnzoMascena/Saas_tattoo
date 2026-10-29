using Tato.Application.Comun;
using Tato.Application.Portfolio;
using Tato.Domain.Portfolio;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// Las fotos de una página contra SQL Server (R-16): subir, reemplazar y quitar la foto o logo del encabezado y la de
/// «Sobre», con los permisos de «Mi página», y la base y los archivos siempre de acuerdo.
/// </summary>
public sealed class PanelFotosPaginaTests : IClassFixture<BaseDeDatosFixture>
{
    private readonly BaseDeDatosFixture _baseDeDatos;

    public PanelFotosPaginaTests(BaseDeDatosFixture baseDeDatos)
    {
        _baseDeDatos = baseDeDatos;
    }

    [Fact]
    public async Task Subir_GuardaLasTresVersionesYLaMuestraEnElDetalle()
    {
        // Arrange
        var (artista, id, version) = await ArtistaConSuPaginaAsync();

        // Act
        var subida = await artista.SubirFotoPaginaAsync(id, LugarFotoPagina.Encabezado, version);

        // Assert: en la base.
        var pagina = await artista.ObtenerPaginaAsync(id);
        Assert.Equal(new FotoConMedidas(subida.Foto.Id, ProcesadorFalso.Ancho, ProcesadorFalso.Alto), pagina.Foto);
        Assert.Null(pagina.FotoSobre);
        Assert.Equal(subida.Version, pagina.Version);

        // Assert: en el almacenamiento, una por tamaño y con la clave de las páginas del estudio (R-16, D-42).
        var claves = artista.Almacenamiento.DeLaFoto(subida.Foto.Id).ToList();
        Assert.Equal(3, claves.Count);
        Assert.All(claves, clave => Assert.StartsWith($"estudios/{artista.EstudioId}/paginas/", clave));
    }

    [Fact]
    public async Task Subir_OtraVez_LaReemplazaYBorraLosArchivosDeLaAnterior()
    {
        // Arrange
        var (artista, id, version) = await ArtistaConSuPaginaAsync();
        var primera = await artista.SubirFotoPaginaAsync(id, LugarFotoPagina.Sobre, version);

        // Act
        var segunda = await artista.SubirFotoPaginaAsync(id, LugarFotoPagina.Sobre, primera.Version);

        // Assert: reemplazar es subir otra, con otro Id; la anterior ya no tiene archivos.
        Assert.NotEqual(primera.Foto.Id, segunda.Foto.Id);
        Assert.Equal(segunda.Foto.Id, (await artista.ObtenerPaginaAsync(id)).FotoSobre?.Id);
        Assert.Empty(artista.Almacenamiento.DeLaFoto(primera.Foto.Id));
        Assert.Equal(3, artista.Almacenamiento.Archivos.Count);
    }

    [Fact]
    public async Task Subir_UnaFotoInvalida_NoGuardaNada()
    {
        // Arrange
        var (artista, id, version) = await ArtistaConSuPaginaAsync();

        // Act & Assert
        await Assert.ThrowsAsync<FotoInvalidaException>(
            () => artista.SubirFotoPaginaAsync(id, LugarFotoPagina.Encabezado, version, new ProcesadorQueRechaza()));

        Assert.Null((await artista.ObtenerPaginaAsync(id)).Foto);
        Assert.Empty(artista.Almacenamiento.Archivos);
    }

    [Fact]
    public async Task Subir_ConUnaVersionVieja_LanzaConflictoYNoDejaArchivosHuerfanos()
    {
        // Arrange: la primera se sube bien.
        var (artista, id, versionAlAbrir) = await ArtistaConSuPaginaAsync();
        var primera = await artista.SubirFotoPaginaAsync(id, LugarFotoPagina.Encabezado, versionAlAbrir);

        // Act & Assert: la segunda llega con la versión vieja. Sus archivos ya se habían guardado.
        await Assert.ThrowsAsync<ConflictoDeVersionException>(
            () => artista.SubirFotoPaginaAsync(id, LugarFotoPagina.Encabezado, versionAlAbrir));

        // Assert: queda la primera, con sus archivos, y nada más.
        Assert.Equal(primera.Foto.Id, (await artista.ObtenerPaginaAsync(id)).Foto?.Id);
        Assert.Equal(3, artista.Almacenamiento.DeLaFoto(primera.Foto.Id).Count());
        Assert.Equal(3, artista.Almacenamiento.Archivos.Count);
    }

    [Fact]
    public async Task Quitar_SacaLaFotoYBorraSusArchivos()
    {
        // Arrange
        var (artista, id, version) = await ArtistaConSuPaginaAsync();
        var subida = await artista.SubirFotoPaginaAsync(id, LugarFotoPagina.Encabezado, version);

        // Act
        var versionNueva = await artista.QuitarFotoPaginaAsync(id, LugarFotoPagina.Encabezado, subida.Version);

        // Assert
        var pagina = await artista.ObtenerPaginaAsync(id);
        Assert.Null(pagina.Foto);
        Assert.Equal(versionNueva, pagina.Version);
        Assert.Empty(artista.Almacenamiento.Archivos);
    }

    [Fact]
    public async Task Quitar_SinFoto_NoFallaYDevuelveLaVersionNueva()
    {
        // Arrange
        var (artista, id, version) = await ArtistaConSuPaginaAsync();

        // Act
        var versionNueva = await artista.QuitarFotoPaginaAsync(id, LugarFotoPagina.Sobre, version);

        // Assert: como ocultar algo que ya está oculto.
        Assert.Equal(versionNueva, (await artista.ObtenerPaginaAsync(id)).Version);
    }

    [Fact]
    public async Task Quitar_ConUnaVersionVieja_LanzaConflictoYNoBorraLosArchivos()
    {
        // Arrange: alguien cambió la página después de que se abrió.
        var (artista, id, version) = await ArtistaConSuPaginaAsync();
        var subida = await artista.SubirFotoPaginaAsync(id, LugarFotoPagina.Encabezado, version);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoDeVersionException>(
            () => artista.QuitarFotoPaginaAsync(id, LugarFotoPagina.Encabezado, version));

        // Assert: primero se guarda la base, así que la foto sigue con sus archivos.
        Assert.Equal(subida.Foto.Id, (await artista.ObtenerPaginaAsync(id)).Foto?.Id);
        Assert.Equal(3, artista.Almacenamiento.DeLaFoto(subida.Foto.Id).Count());
    }

    [Fact]
    public async Task Subir_EnLaPaginaDeOtroArtistaAunqueSeaDuenio_LanzaAccesoDenegadoYNoGuardaNada()
    {
        // Arrange: Belén es Dueña, pero la página es de Emma (D-51).
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var belen = duenio.Como(await duenio.CrearArtistaAsync());
        var emma = duenio.Como(await duenio.CrearArtistaAsync());
        var id = await emma.CrearPaginaAsync(delEstudio: false, new DatosPagina(NombreUnico("Emma")));
        var version = (await emma.ObtenerPaginaAsync(id)).Version;

        // Act & Assert
        await Assert.ThrowsAsync<AccesoDenegadoException>(
            () => belen.SubirFotoPaginaAsync(id, LugarFotoPagina.Encabezado, version));
        await Assert.ThrowsAsync<AccesoDenegadoException>(
            () => belen.QuitarFotoPaginaAsync(id, LugarFotoPagina.Encabezado, version));

        Assert.Empty(belen.Almacenamiento.Archivos);
    }

    [Fact]
    public async Task Subir_EnLaDelEstudio_UnTatuadorQueNoEsDuenio_LanzaAccesoDenegado()
    {
        // Arrange: los datos de la página del estudio son de los Dueños (doc 07, 5).
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var tatuador = duenio.Como(await duenio.CrearArtistaAsync(esDuenio: false));
        var id = await duenio.CrearPaginaAsync(delEstudio: true, new DatosPagina(NombreUnico("Alkimya")));
        var version = (await duenio.ObtenerPaginaAsync(id)).Version;

        // Act & Assert
        await Assert.ThrowsAsync<AccesoDenegadoException>(
            () => tatuador.SubirFotoPaginaAsync(id, LugarFotoPagina.Encabezado, version));
    }

    [Fact]
    public async Task Subir_EnUnaPaginaDeOtroEstudio_LanzaNoEncontrado()
    {
        // Arrange: un Dueño de otro estudio, que en el suyo sí podría gestionar la página del estudio.
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var id = await duenio.CrearPaginaAsync(delEstudio: true, new DatosPagina(NombreUnico("Alkimya")));
        var version = (await duenio.ObtenerPaginaAsync(id)).Version;
        var otroEstudio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());

        // Act & Assert: 404, igual que si no existiera (D-42).
        await Assert.ThrowsAsync<NoEncontradoException>(
            () => otroEstudio.SubirFotoPaginaAsync(id, LugarFotoPagina.Encabezado, version));
        await Assert.ThrowsAsync<NoEncontradoException>(
            () => otroEstudio.QuitarFotoPaginaAsync(id, LugarFotoPagina.Encabezado, version));
    }

    [Fact]
    public async Task LeerFoto_DeUnaPagina_LaEncuentraConElEstudioDeLaSesionYNoConOtro()
    {
        // Arrange
        var (artista, id, version) = await ArtistaConSuPaginaAsync();
        var subida = await artista.SubirFotoPaginaAsync(id, LugarFotoPagina.Sobre, version);

        // Act
        await using var delMismoEstudio = await artista.LeerFotoAsync(subida.Foto.Id, TamanioFoto.Galeria);
        var otroEstudio = new LeerFoto(new EstudioFalso(Guid.NewGuid()), artista.Almacenamiento);
        await using var deOtroEstudio = await otroEstudio.EjecutarAsync(subida.Foto.Id, TamanioFoto.Galeria);

        // Assert: con el mismo almacenamiento, otro estudio arma otra clave y no la encuentra (D-42).
        Assert.NotNull(delMismoEstudio);
        Assert.Null(deOtroEstudio);
    }

    /// <summary>
    /// Un artista con su página recién creada y la versión con la que se abrió.
    /// </summary>
    private async Task<(Panel Artista, Guid Id, string Version)> ArtistaConSuPaginaAsync()
    {
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var artista = duenio.Como(await duenio.CrearArtistaAsync());
        var id = await artista.CrearPaginaAsync(delEstudio: false, new DatosPagina(NombreUnico("Ink Mortal")));
        return (artista, id, (await artista.ObtenerPaginaAsync(id)).Version);
    }

    /// <summary>
    /// Todos los tests de la clase comparten la base: cada página necesita un nombre, y una dirección, únicos.
    /// </summary>
    private static string NombreUnico(string nombre) => $"{nombre} {Guid.NewGuid():N}";
}
