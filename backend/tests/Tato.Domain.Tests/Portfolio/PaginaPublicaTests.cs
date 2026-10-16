namespace Tato.Domain.Tests.Portfolio;

using Tato.Domain.Portfolio;
using Xunit;

/// <summary>
/// La página pública de un estudio o de un artista (D-49): dirección, visibilidad y datos del encabezado.
/// </summary>
public class PaginaPublicaTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CrearDeArtista_ArmaLaDireccionConElNombreYArrancaOculta()
    {
        // Arrange
        var tatuadorId = Guid.NewGuid();

        // Act
        var pagina = PaginaPublica.CrearDeArtista(Guid.NewGuid(), tatuadorId, new DatosPagina("Ink Mortal"), Ahora);

        // Assert
        Assert.Equal("inkmortal", pagina.Direccion.Valor);
        Assert.Equal(tatuadorId, pagina.TatuadorId);
        Assert.False(pagina.EsDelEstudio);
        Assert.False(pagina.Visible);
    }

    [Fact]
    public void CrearDelEstudio_NoTieneArtista()
    {
        var pagina = PaginaPublica.CrearDelEstudio(Guid.NewGuid(), new DatosPagina("Alkimya Tattoo"), Ahora);

        Assert.True(pagina.EsDelEstudio);
        Assert.Null(pagina.TatuadorId);
    }

    [Fact]
    public void Editar_CambiarElNombre_NoCambiaLaDireccion()
    {
        // Arrange
        var pagina = CrearDeArtista("Ink Mortal");

        // Act
        pagina.Editar(new DatosPagina("Belén Ink"), Ahora.AddHours(1));

        // Assert: los links ya compartidos siguen funcionando (RN-BLG-07).
        Assert.Equal("Belén Ink", pagina.Nombre);
        Assert.Equal("inkmortal", pagina.Direccion.Valor);
    }

    [Fact]
    public void CambiarDireccion_UsaLaNueva()
    {
        var pagina = CrearDeArtista("Ink Mortal");

        pagina.CambiarDireccion(DireccionPublica.Crear("belen-ink"), Ahora.AddHours(1));

        Assert.Equal("belen-ink", pagina.Direccion.Valor);
    }

    [Fact]
    public void Mostrar_YOcultar_CambianLaVisibilidad()
    {
        var pagina = CrearDeArtista("Ink Mortal");

        pagina.Mostrar(Ahora);
        Assert.True(pagina.Visible);

        pagina.Ocultar(Ahora);
        Assert.False(pagina.Visible);
    }

    [Fact]
    public void Editar_InstagramConArroba_LoGuardaSinArroba()
    {
        var pagina = CrearDeArtista("Ink Mortal");

        pagina.Editar(new DatosPagina("Ink Mortal", Instagram: "@ink.mortall"), Ahora);

        Assert.Equal("ink.mortall", pagina.Instagram);
    }

    [Fact]
    public void Editar_WhatsAppConEspaciosYGuiones_LoGuardaEnFormatoInternacional()
    {
        var pagina = CrearDeArtista("Ink Mortal");

        pagina.Editar(new DatosPagina("Ink Mortal", WhatsApp: "+54 9 221 555-0000"), Ahora);

        Assert.Equal("+5492215550000", pagina.WhatsApp);
    }

    [Theory]
    [InlineData("221 555-0000")]
    [InlineData("+0 221 555 0000")]
    [InlineData("+54 9 221 555-0000 int 2")]
    [InlineData("+123")]
    public void Editar_WhatsAppInvalido_LanzaArgumentException(string whatsApp)
    {
        var pagina = CrearDeArtista("Ink Mortal");

        Assert.Throws<ArgumentException>(
            () => pagina.Editar(new DatosPagina("Ink Mortal", WhatsApp: whatsApp), Ahora));
    }

    [Fact]
    public void Editar_UbicacionEnLaPaginaDeUnArtista_LanzaArgumentException()
    {
        var pagina = CrearDeArtista("Ink Mortal");

        Assert.Throws<ArgumentException>(
            () => pagina.Editar(new DatosPagina("Ink Mortal", Ubicacion: "Calle 7 nº 1234"), Ahora));
    }

    [Fact]
    public void CrearDelEstudio_ConUbicacionYHorarios_LosGuarda()
    {
        var datos = new DatosPagina("Alkimya Tattoo", Ubicacion: "Calle 7 nº 1234, La Plata", Horarios: "Lun a sáb, 11 a 20");

        var pagina = PaginaPublica.CrearDelEstudio(Guid.NewGuid(), datos, Ahora);

        Assert.Equal("Calle 7 nº 1234, La Plata", pagina.Ubicacion);
        Assert.Equal("Lun a sáb, 11 a 20", pagina.Horarios);
    }

    [Fact]
    public void Crear_ConUnaBioDeMasDe150Caracteres_LanzaArgumentException()
    {
        var datos = new DatosPagina("Ink Mortal", Bio: new string('a', 151));

        Assert.Throws<ArgumentException>(
            () => PaginaPublica.CrearDeArtista(Guid.NewGuid(), Guid.NewGuid(), datos, Ahora));
    }

    private static PaginaPublica CrearDeArtista(string nombre) =>
        PaginaPublica.CrearDeArtista(Guid.NewGuid(), Guid.NewGuid(), new DatosPagina(nombre), Ahora);
}
