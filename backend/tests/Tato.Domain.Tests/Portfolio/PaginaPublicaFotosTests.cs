namespace Tato.Domain.Tests.Portfolio;

using Tato.Domain.Portfolio;
using Xunit;

/// <summary>
/// Las dos fotos de una página pública (R-16): la foto o logo del encabezado y la de «Sobre».
/// </summary>
public class PaginaPublicaFotosTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CambiarFoto_SinFotoAnterior_LaPoneYDevuelveNull()
    {
        // Arrange
        var pagina = CrearPagina();
        var foto = FotoPagina.Crear(1080, 1350);

        // Act
        var anterior = pagina.CambiarFoto(LugarFotoPagina.Encabezado, foto, Ahora);

        // Assert
        Assert.Null(anterior);
        Assert.Equal(foto, pagina.Foto);
    }

    [Fact]
    public void CambiarFoto_ConFotoAnterior_LaReemplazaYDevuelveLaAnterior()
    {
        // Arrange
        var pagina = CrearPagina();
        var vieja = FotoPagina.Crear(1080, 1350);
        var nueva = FotoPagina.Crear(800, 800);
        pagina.CambiarFoto(LugarFotoPagina.Encabezado, vieja, Ahora);

        // Act
        var anterior = pagina.CambiarFoto(LugarFotoPagina.Encabezado, nueva, Ahora.AddHours(1));

        // Assert: la vieja vuelve para que el caso de uso borre sus archivos.
        Assert.Equal(vieja, anterior);
        Assert.Equal(nueva, pagina.Foto);
        Assert.Equal(Ahora.AddHours(1), pagina.ActualizadaEn);
    }

    [Fact]
    public void CambiarFoto_EnSobre_NoTocaLaDelEncabezado()
    {
        var pagina = CrearPagina();
        var encabezado = FotoPagina.Crear(400, 400);
        var sobre = FotoPagina.Crear(1080, 1350);
        pagina.CambiarFoto(LugarFotoPagina.Encabezado, encabezado, Ahora);

        pagina.CambiarFoto(LugarFotoPagina.Sobre, sobre, Ahora);

        Assert.Equal(encabezado, pagina.Foto);
        Assert.Equal(sobre, pagina.FotoSobre);
        Assert.Equal(sobre, pagina.FotoEn(LugarFotoPagina.Sobre));
    }

    [Fact]
    public void QuitarFoto_DevuelveLaQuitadaYDejaElLugarVacio()
    {
        var pagina = CrearPagina();
        var foto = FotoPagina.Crear(1080, 1350);
        pagina.CambiarFoto(LugarFotoPagina.Sobre, foto, Ahora);

        var quitada = pagina.QuitarFoto(LugarFotoPagina.Sobre, Ahora);

        Assert.Equal(foto, quitada);
        Assert.Null(pagina.FotoSobre);
    }

    [Fact]
    public void QuitarFoto_SinFoto_DevuelveNull()
    {
        var pagina = CrearPagina();

        Assert.Null(pagina.QuitarFoto(LugarFotoPagina.Encabezado, Ahora));
    }

    [Fact]
    public void CambiarFoto_EnUnLugarQueNoExiste_LanzaArgumentException()
    {
        var pagina = CrearPagina();

        Assert.ThrowsAny<ArgumentException>(
            () => pagina.CambiarFoto((LugarFotoPagina)99, FotoPagina.Crear(400, 400), Ahora));
    }

    [Fact]
    public void CambiarFoto_ConUnaFechaQueNoEstaEnUtc_LanzaArgumentException()
    {
        var pagina = CrearPagina();
        var enArgentina = new DateTimeOffset(2026, 10, 29, 9, 0, 0, TimeSpan.FromHours(-3));

        Assert.Throws<ArgumentException>(
            () => pagina.CambiarFoto(LugarFotoPagina.Encabezado, FotoPagina.Crear(400, 400), enArgentina));
    }

    [Fact]
    public void FotoPaginaCrear_GeneraUnIdNuevoCadaVez()
    {
        Assert.NotEqual(FotoPagina.Crear(400, 400).Id, FotoPagina.Crear(400, 400).Id);
    }

    [Theory]
    [InlineData(0, 400)]
    [InlineData(400, 0)]
    [InlineData(-1, 400)]
    public void FotoPaginaCrear_ConMedidasInvalidas_LanzaArgumentOutOfRangeException(int ancho, int alto)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FotoPagina.Crear(ancho, alto));
    }

    private static PaginaPublica CrearPagina() =>
        PaginaPublica.CrearDeArtista(Guid.NewGuid(), Guid.NewGuid(), new DatosPagina("Ink Mortal"), Ahora);
}
