namespace Tato.Domain.Tests.Portfolio;

using Tato.Domain.Comun;
using Tato.Domain.Portfolio;
using Xunit;

/// <summary>
/// Fotos de una publicación: agregar, quitar, ordenar y elegir la portada (RN-BLG-04).
/// </summary>
public class PublicacionFotosTests
{
    private static readonly DateTimeOffset Ahora = PublicacionBuilder.Ahora;
    private const int MaxFotos = PublicacionBuilder.MaxFotos;

    [Fact]
    public void AgregarFoto_PrimeraFoto_QuedaComoPortada()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().Build();
        var despues = Ahora.AddMinutes(5);

        // Act
        var foto = publicacion.AgregarFoto(1600, 1200, MaxFotos, despues);

        // Assert
        Assert.NotEqual(Guid.Empty, foto.Id);
        Assert.Equal(publicacion.Id, foto.PublicacionId);
        Assert.Equal(publicacion.EstudioId, foto.EstudioId);
        Assert.Equal(0, foto.Orden);
        Assert.Equal(1600, foto.Ancho);
        Assert.Equal(1200, foto.Alto);
        Assert.Equal(despues, foto.SubidaEn);
        Assert.Same(foto, publicacion.Portada);
        Assert.Equal(despues, publicacion.ActualizadaEn);
    }

    [Fact]
    public void AgregarFoto_VariasFotos_QuedanAlFinalEnOrden()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().Build();

        // Act
        var primera = publicacion.AgregarFoto(1600, 1200, MaxFotos, Ahora);
        var segunda = publicacion.AgregarFoto(1200, 1600, MaxFotos, Ahora);
        var tercera = publicacion.AgregarFoto(1600, 1600, MaxFotos, Ahora);

        // Assert
        Assert.Equal(new[] { primera.Id, segunda.Id, tercera.Id }, publicacion.Fotos.Select(f => f.Id));
        Assert.Equal(new[] { 0, 1, 2 }, publicacion.Fotos.Select(f => f.Orden));
        Assert.Same(primera, publicacion.Portada);
    }

    [Fact]
    public void AgregarFoto_LlegandoAlMaximo_SePermite()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().ConFotos(2).Build();

        // Act
        publicacion.AgregarFoto(1600, 1200, maxFotos: 3, Ahora);

        // Assert
        Assert.Equal(3, publicacion.Fotos.Count);
    }

    [Fact]
    public void AgregarFoto_PasandoElMaximo_LanzaReglaDeNegocio()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().ConFotos(3).Build();

        // Act & Assert
        var ex = Assert.Throws<ReglaDeNegocioException>(() =>
            publicacion.AgregarFoto(1600, 1200, maxFotos: 3, Ahora));
        Assert.Equal("RN-BLG-04", ex.Regla);
        Assert.Contains("hasta 3 fotos", ex.Message);
        Assert.Equal(3, publicacion.Fotos.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AgregarFoto_ConMaximoInvalido_LanzaArgumentOutOfRange(int maxFotos)
    {
        var publicacion = new PublicacionBuilder().Build();

        Assert.Throws<ArgumentOutOfRangeException>(() => publicacion.AgregarFoto(1600, 1200, maxFotos, Ahora));
    }

    [Theory]
    [InlineData(0, 1200)]
    [InlineData(1600, 0)]
    [InlineData(-5, 1200)]
    public void AgregarFoto_ConMedidasInvalidas_LanzaArgumentOutOfRange(int ancho, int alto)
    {
        // Arrange
        var publicacion = new PublicacionBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => publicacion.AgregarFoto(ancho, alto, MaxFotos, Ahora));
        Assert.Empty(publicacion.Fotos);
    }

    [Fact]
    public void QuitarFoto_DelMedio_CorreLasSiguientes()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().ConFotos(3).Build();
        var (primera, segunda, tercera) = (publicacion.Fotos[0], publicacion.Fotos[1], publicacion.Fotos[2]);

        // Act
        var quitada = publicacion.QuitarFoto(segunda.Id, Ahora.AddHours(1));

        // Assert
        Assert.Same(segunda, quitada);
        Assert.Equal(new[] { primera.Id, tercera.Id }, publicacion.Fotos.Select(f => f.Id));
        Assert.Equal(new[] { 0, 1 }, publicacion.Fotos.Select(f => f.Orden));
        Assert.Equal(Ahora.AddHours(1), publicacion.ActualizadaEn);
    }

    [Fact]
    public void QuitarFoto_LaPortada_LaSiguientePasaASerPortada()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().ConFotos(2).Build();
        var segunda = publicacion.Fotos[1];

        // Act
        publicacion.QuitarFoto(publicacion.Fotos[0].Id, Ahora);

        // Assert
        Assert.Same(segunda, publicacion.Portada);
        Assert.Equal(0, segunda.Orden);
    }

    [Fact]
    public void QuitarFoto_UltimaDeUnBorrador_SePermite()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().ConFotos(1).Build();

        // Act
        publicacion.QuitarFoto(publicacion.Fotos[0].Id, Ahora);

        // Assert
        Assert.Empty(publicacion.Fotos);
        Assert.Null(publicacion.Portada);
    }

    [Fact]
    public void QuitarFoto_UltimaDeUnaPublicada_LanzaReglaDeNegocio()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().Publicada().ConFotos(1).Build();

        // Act & Assert
        var ex = Assert.Throws<ReglaDeNegocioException>(() => publicacion.QuitarFoto(publicacion.Fotos[0].Id, Ahora));
        Assert.Equal("RN-BLG-04", ex.Regla);
        Assert.Single(publicacion.Fotos);
    }

    [Fact]
    public void QuitarFoto_UltimaDeUnaOculta_SePermite()
    {
        // Oculta no se ve en el sitio: puede quedarse sin fotos hasta volver a publicarse.
        var publicacion = new PublicacionBuilder().Publicada().ConFotos(1).Build();
        publicacion.Ocultar(Ahora);

        publicacion.QuitarFoto(publicacion.Fotos[0].Id, Ahora);

        Assert.Empty(publicacion.Fotos);
    }

    [Fact]
    public void QuitarFoto_DeOtraPublicacion_LanzaArgumentException()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().ConFotos(1).Build();
        var otra = new PublicacionBuilder().ConFotos(1).Build();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => publicacion.QuitarFoto(otra.Fotos[0].Id, Ahora));
        Assert.Contains("no pertenece a esta publicación", ex.Message);
    }

    [Fact]
    public void ElegirPortada_TerceraFoto_PasaAlPrincipioYLasDemasSeCorren()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().ConFotos(3).Build();
        var (primera, segunda, tercera) = (publicacion.Fotos[0], publicacion.Fotos[1], publicacion.Fotos[2]);

        // Act
        publicacion.ElegirPortada(tercera.Id, Ahora.AddHours(1));

        // Assert
        Assert.Same(tercera, publicacion.Portada);
        Assert.Equal(new[] { tercera.Id, primera.Id, segunda.Id }, publicacion.Fotos.Select(f => f.Id));
        Assert.Equal(new[] { 0, 1, 2 }, publicacion.Fotos.Select(f => f.Orden));
        Assert.Equal(Ahora.AddHours(1), publicacion.ActualizadaEn);
    }

    [Fact]
    public void ElegirPortada_FotoDeOtraPublicacion_LanzaArgumentException()
    {
        var publicacion = new PublicacionBuilder().ConFotos(2).Build();

        Assert.Throws<ArgumentException>(() => publicacion.ElegirPortada(Guid.NewGuid(), Ahora));
    }

    [Fact]
    public void Reordenar_ConTodasLasFotos_AplicaElOrden()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().ConFotos(3).Build();
        var (primera, segunda, tercera) = (publicacion.Fotos[0], publicacion.Fotos[1], publicacion.Fotos[2]);

        // Act
        publicacion.Reordenar([segunda.Id, tercera.Id, primera.Id], Ahora.AddHours(1));

        // Assert
        Assert.Equal(new[] { segunda.Id, tercera.Id, primera.Id }, publicacion.Fotos.Select(f => f.Id));
        Assert.Equal(new[] { 0, 1, 2 }, publicacion.Fotos.Select(f => f.Orden));
        Assert.Same(segunda, publicacion.Portada);
        Assert.Equal(Ahora.AddHours(1), publicacion.ActualizadaEn);
    }

    [Fact]
    public void Reordenar_FaltaUnaFoto_LanzaArgumentExceptionYNoCambia()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().ConFotos(3).Build();
        var ordenOriginal = publicacion.Fotos.Select(f => f.Id).ToList();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => publicacion.Reordenar([ordenOriginal[2], ordenOriginal[0]], Ahora));
        Assert.Equal(ordenOriginal, publicacion.Fotos.Select(f => f.Id));
    }

    [Fact]
    public void Reordenar_FotoRepetida_LanzaArgumentException()
    {
        var publicacion = new PublicacionBuilder().ConFotos(2).Build();
        var primera = publicacion.Fotos[0].Id;

        Assert.Throws<ArgumentException>(() => publicacion.Reordenar([primera, primera], Ahora));
    }

    [Fact]
    public void Reordenar_FotoDeOtraPublicacion_LanzaArgumentException()
    {
        var publicacion = new PublicacionBuilder().ConFotos(2).Build();

        Assert.Throws<ArgumentException>(() =>
            publicacion.Reordenar([publicacion.Fotos[0].Id, Guid.NewGuid()], Ahora));
    }
}
