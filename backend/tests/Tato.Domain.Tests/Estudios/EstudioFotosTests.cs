namespace Tato.Domain.Tests.Estudios;

using Tato.Domain.Estudios;
using Xunit;

public class EstudioFotosTests
{
    [Fact]
    public void Crear_SinMaxFotos_UsaElDefectoDe10()
    {
        // Act
        var estudio = Estudio.Crear("Black Dragon", "starter");

        // Assert
        Assert.Equal(10, estudio.MaxFotosPorPublicacion);
    }

    [Fact]
    public void Crear_ConMaxFotos_LoGuarda()
    {
        // Act
        var estudio = Estudio.Crear("Black Dragon", "starter", maxFotosPorPublicacion: 5);

        // Assert
        Assert.Equal(5, estudio.MaxFotosPorPublicacion);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Crear_ConMaxFotosNoPositivo_LanzaArgumentException(int maxFotos)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Estudio.Crear("Black Dragon", "starter", maxFotosPorPublicacion: maxFotos));
        Assert.Contains("mayor a 0", ex.Message);
    }

    [Fact]
    public void ConfigurarMaxFotosPorPublicacion_ConValorValido_Cambia()
    {
        // Arrange
        var estudio = Estudio.Crear("Black Dragon", "starter");

        // Act
        estudio.ConfigurarMaxFotosPorPublicacion(20);

        // Assert
        Assert.Equal(20, estudio.MaxFotosPorPublicacion);
    }

    [Fact]
    public void ConfigurarMaxFotosPorPublicacion_ConCero_LanzaYNoCambia()
    {
        // Arrange
        var estudio = Estudio.Crear("Black Dragon", "starter");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => estudio.ConfigurarMaxFotosPorPublicacion(0));
        Assert.Equal(10, estudio.MaxFotosPorPublicacion);
    }
}