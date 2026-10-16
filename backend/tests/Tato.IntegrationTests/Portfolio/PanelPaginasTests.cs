using Tato.Application.Comun;
using Tato.Domain.Comun;
using Tato.Domain.Portfolio;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// «Mi página» contra SQL Server: crear la página del estudio o la propia, editarla, cambiar su dirección y
/// mostrarla u ocultarla. Cada página la gestiona su artista, y la del estudio los Dueños (doc 07, 5). La dirección
/// es única en toda la plataforma (RN-BLG-07).
/// <para>
/// Todos los tests de la clase comparten la base, así que cada uno usa nombres únicos: si no, sus direcciones
/// chocarían entre sí.
/// </para>
/// </summary>
public sealed class PanelPaginasTests : IClassFixture<BaseDeDatosFixture>
{
    private readonly BaseDeDatosFixture _baseDeDatos;

    public PanelPaginasTests(BaseDeDatosFixture baseDeDatos)
    {
        _baseDeDatos = baseDeDatos;
    }

    [Fact]
    public async Task Crear_LaDelEstudio_NaceOcultaYConLaDireccionDelNombre()
    {
        // Arrange
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var nombre = NombreUnico("Alkimya");
        var datos = new DatosPagina(nombre, Horarios: "Lun a sáb, 11 a 20");

        // Act
        var id = await duenio.CrearPaginaAsync(delEstudio: true, datos);

        // Assert
        var pagina = await duenio.ObtenerPaginaAsync(id);
        Assert.True(pagina.EsDelEstudio);
        Assert.Equal(DireccionPublica.DesdeNombre(nombre).Valor, pagina.Direccion);
        Assert.Equal("Lun a sáb, 11 a 20", pagina.Horarios);
        Assert.False(pagina.Visible);
    }

    [Fact]
    public async Task Crear_LaDelEstudio_UnTatuadorQueNoEsDuenio_LanzaAccesoDenegado()
    {
        // Arrange
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var tatuador = duenio.Como(await duenio.CrearArtistaAsync(esDuenio: false));

        // Act & Assert
        await Assert.ThrowsAsync<AccesoDenegadoException>(
            () => tatuador.CrearPaginaAsync(delEstudio: true, new DatosPagina(NombreUnico("Intento"))));
    }

    [Fact]
    public async Task Crear_LaPropia_SinPerfilDeTatuador_LanzaAccesoDenegado()
    {
        // Arrange: un Dueño que no tatúa no tiene página de artista.
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());

        // Act & Assert
        await Assert.ThrowsAsync<AccesoDenegadoException>(
            () => duenio.CrearPaginaAsync(delEstudio: false, new DatosPagina(NombreUnico("Intento"))));
    }

    [Fact]
    public async Task Crear_LaDelEstudioPorSegundaVez_LanzaReglaD49()
    {
        // Arrange
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        await duenio.CrearPaginaAsync(delEstudio: true, new DatosPagina(NombreUnico("Alkimya")));

        // Act
        var excepcion = await Assert.ThrowsAsync<ReglaDeNegocioException>(
            () => duenio.CrearPaginaAsync(delEstudio: true, new DatosPagina(NombreUnico("Otra"))));

        // Assert
        Assert.Equal("D-49", excepcion.Regla);
    }

    [Fact]
    public async Task Crear_ConLaDireccionDeUnaPaginaDeOtroEstudio_LanzaReglaRNBLG07()
    {
        // Arrange: la dirección es única en toda la plataforma, no solo dentro del estudio.
        var nombre = NombreUnico("Ink Mortal");
        var otroEstudio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        await otroEstudio.CrearPaginaAsync(delEstudio: true, new DatosPagina(nombre));
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var artista = duenio.Como(await duenio.CrearArtistaAsync());

        // Act
        var excepcion = await Assert.ThrowsAsync<ReglaDeNegocioException>(
            () => artista.CrearPaginaAsync(delEstudio: false, new DatosPagina(nombre)));

        // Assert
        Assert.Equal("RN-BLG-07", excepcion.Regla);
    }

    [Fact]
    public async Task MisPaginas_UnArtistaDuenio_VeLaSuyaYLaDelEstudio_PeroNoLaDeOtroArtista()
    {
        // Arrange: como el estudio piloto, dos artistas que también son Dueños.
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var belen = duenio.Como(await duenio.CrearArtistaAsync());
        var emma = duenio.Como(await duenio.CrearArtistaAsync());
        var deBelen = await belen.CrearPaginaAsync(delEstudio: false, new DatosPagina(NombreUnico("Ink Mortal")));
        var delEstudio = await belen.CrearPaginaAsync(delEstudio: true, new DatosPagina(NombreUnico("Alkimya")));
        await emma.CrearPaginaAsync(delEstudio: false, new DatosPagina(NombreUnico("Emma")));

        // Act
        var paginas = await belen.MisPaginasAsync();

        // Assert: primero la suya y después la del estudio.
        Assert.Equal(new[] { deBelen, delEstudio }, paginas.Select(p => p.Id));
    }

    [Fact]
    public async Task Editar_ElArtista_CambiaLosDatosPeroNoLaDireccion()
    {
        // Arrange
        var (artista, id, version) = await ArtistaConSuPaginaAsync();
        var direccion = (await artista.ObtenerPaginaAsync(id)).Direccion;

        // Act
        await artista.EditarPaginaAsync(
            id, new DatosPagina(NombreUnico("Nuevo nombre"), Instagram: "@ink.mortall"), version);

        // Assert: la dirección sigue igual, para no romper los links ya compartidos (RN-BLG-07).
        var pagina = await artista.ObtenerPaginaAsync(id);
        Assert.StartsWith("Nuevo nombre", pagina.Nombre);
        Assert.Equal("ink.mortall", pagina.Instagram);
        Assert.Equal(direccion, pagina.Direccion);
    }

    [Fact]
    public async Task Editar_LaPaginaDeOtroArtistaAunqueSeaDuenio_LanzaAccesoDenegado()
    {
        // Arrange: Belén es Dueña, pero la página es de Emma.
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var belen = duenio.Como(await duenio.CrearArtistaAsync());
        var emma = duenio.Como(await duenio.CrearArtistaAsync());
        var id = await emma.CrearPaginaAsync(delEstudio: false, new DatosPagina(NombreUnico("Emma")));
        var version = (await emma.ObtenerPaginaAsync(id)).Version;

        // Act & Assert
        await Assert.ThrowsAsync<AccesoDenegadoException>(
            () => belen.EditarPaginaAsync(id, new DatosPagina("Intento"), version));
    }

    [Fact]
    public async Task Editar_ConUnaVersionVieja_LanzaConflicto()
    {
        // Arrange: alguien guardó un cambio después de que se abrió la página.
        var (artista, id, version) = await ArtistaConSuPaginaAsync();
        await artista.EditarPaginaAsync(id, new DatosPagina(NombreUnico("Primer cambio")), version);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoDeVersionException>(
            () => artista.EditarPaginaAsync(id, new DatosPagina(NombreUnico("Segundo cambio")), version));
    }

    [Fact]
    public async Task CambiarDireccion_AUnaLibre_LaCambia()
    {
        // Arrange
        var (artista, id, version) = await ArtistaConSuPaginaAsync();
        var nueva = $"ink-{Guid.NewGuid():N}";

        // Act
        await artista.CambiarDireccionAsync(id, nueva, version);

        // Assert
        Assert.Equal(nueva, (await artista.ObtenerPaginaAsync(id)).Direccion);
    }

    [Fact]
    public async Task CambiarDireccion_AUnaEnUso_LanzaReglaRNBLG07()
    {
        // Arrange: la dirección ya la usa la página del estudio.
        var (artista, id, version) = await ArtistaConSuPaginaAsync();
        var delEstudio = await artista.CrearPaginaAsync(delEstudio: true, new DatosPagina(NombreUnico("Alkimya")));
        var enUso = (await artista.ObtenerPaginaAsync(delEstudio)).Direccion;

        // Act
        var excepcion = await Assert.ThrowsAsync<ReglaDeNegocioException>(
            () => artista.CambiarDireccionAsync(id, enUso, version));

        // Assert
        Assert.Equal("RN-BLG-07", excepcion.Regla);
    }

    [Fact]
    public async Task MostrarYOcultar_CambianSiSeVeEnElSitio()
    {
        // Arrange
        var (artista, id, version) = await ArtistaConSuPaginaAsync();

        // Act & Assert
        version = await artista.MostrarPaginaAsync(id, version);
        Assert.True((await artista.ObtenerPaginaAsync(id)).Visible);

        await artista.OcultarPaginaAsync(id, version);
        Assert.False((await artista.ObtenerPaginaAsync(id)).Visible);
    }

    [Fact]
    public async Task Agregar_UnaDireccionRepetidaSinPasarPorElCasoDeUso_LaBaseLaRechaza()
    {
        // Arrange: dos estudios que, al mismo tiempo, eligen el mismo nombre. Los dos pasaron el control del caso de
        // uso; se simula guardando directo con el repositorio.
        var nombre = NombreUnico("Mismo nombre");
        var primero = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var segundo = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        await primero.AgregarPaginaSinControlesAsync(new DatosPagina(nombre));

        // Act & Assert: el índice único de la base es la garantía final.
        await Assert.ThrowsAsync<ConflictoDeVersionException>(
            () => segundo.AgregarPaginaSinControlesAsync(new DatosPagina(nombre)));
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
    /// Un nombre que no se repite entre tests: su dirección tampoco.
    /// </summary>
    private static string NombreUnico(string nombre) => $"{nombre} {Guid.NewGuid():N}";
}
