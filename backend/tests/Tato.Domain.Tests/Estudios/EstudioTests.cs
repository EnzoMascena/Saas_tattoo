namespace Tato.Domain.Tests.Estudios;

using Tato.Domain.Estudios;
using Xunit;

public class EstudioTests
{
    [Fact]
    public void Crear_ConDatosValidos_DevuelveEstudio()
    {
        // Arrange & Act
        var estudio = Estudio.Crear("Black Dragon", "starter");

        // Assert
        Assert.NotEqual(Guid.Empty, estudio.Id);
        Assert.Equal("Black Dragon", estudio.Nombre);
        Assert.Equal("starter", estudio.Plan);
        Assert.Equal("America/Argentina/Buenos_Aires", estudio.ZonaHoraria);
        Assert.Equal(14, estudio.DiasVencimientoSenia);
        Assert.Equal(2, estudio.MaxReagendamientos);
    }

    [Fact]
    public void Crear_ConNombreVacio_LanzaArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => Estudio.Crear("", "starter"));
        Assert.Contains("no puede estar vacio", ex.Message);
    }

    [Fact]
    public void Crear_ConNombreMayorA100Caracteres_LanzaArgumentException()
    {
        // Arrange
        var nombreLargo = new string('a', 101);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => Estudio.Crear(nombreLargo, "starter"));
        Assert.Contains("no puede exceder 100 caracteres", ex.Message);
    }

    [Fact]
    public void Crear_ConPlanVacio_LanzaArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => Estudio.Crear("Black Dragon", ""));
        Assert.Contains("no puede estar vacio", ex.Message);
    }

    [Fact]
    public void Crear_ConParametrosPersonalizados_LosUsa()
    {
        // Act
        var estudio = Estudio.Crear(
            "Estudio XYZ",
            "pro",
            zonaHoraria: "America/New_York",
            diasVencimientoSenia: 7,
            maxReagendamientos: 3);

        // Assert
        Assert.Equal("America/New_York", estudio.ZonaHoraria);
        Assert.Equal(7, estudio.DiasVencimientoSenia);
        Assert.Equal(3, estudio.MaxReagendamientos);
    }

    [Fact]
    public void Crear_ConDiasVencimientoMenorOIgualACero_LanzaArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Estudio.Crear("Test", "starter", diasVencimientoSenia: 0));
        Assert.Contains("deben ser mayor a 0", ex.Message);
    }

    [Fact]
    public void Crear_ConMaxReagendamientosNegativo_LanzaArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Estudio.Crear("Test", "starter", maxReagendamientos: -1));
        Assert.Contains("no puede ser negativo", ex.Message);
    }

    [Fact]
    public void ConfigurarParametros_ConValoresValidos_Cambia()
    {
        // Arrange
        var estudio = Estudio.Crear("Test", "starter");

        // Act
        estudio.ConfigurarParametros(diasVencimientoSenia: 21, maxReagendamientos: 5);

        // Assert
        Assert.Equal(21, estudio.DiasVencimientoSenia);
        Assert.Equal(5, estudio.MaxReagendamientos);
    }

    [Fact]
    public void ConfigurarParametros_ConDiasVencimientoMenorOIgualACero_LanzaArgumentException()
    {
        // Arrange
        var estudio = Estudio.Crear("Test", "starter");

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            estudio.ConfigurarParametros(diasVencimientoSenia: 0, maxReagendamientos: 2));
        Assert.Contains("deben ser mayor a 0", ex.Message);
    }

    [Fact]
    public void ConfigurarParametros_ConMaxReagendamientosNegativo_LanzaArgumentException()
    {
        // Arrange
        var estudio = Estudio.Crear("Test", "starter");

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            estudio.ConfigurarParametros(diasVencimientoSenia: 14, maxReagendamientos: -1));
        Assert.Contains("no puede ser negativo", ex.Message);
    }

    [Fact]
    public void Crear_ConPlanMuyLargo_LanzaArgumentException()
    {
        // Arrange
        var plan = new string('a', Estudio.MaxPlan + 1);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => Estudio.Crear("Test", plan));
        Assert.Contains($"no puede exceder {Estudio.MaxPlan} caracteres", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConZonaHorariaVacia_UsaLaZonaPorDefecto(string zona)
    {
        // Act
        var estudio = Estudio.Crear("Test", "starter", zonaHoraria: zona);

        // Assert
        Assert.Equal(Estudio.ZonaHorariaPorDefecto, estudio.ZonaHoraria);
    }

    [Fact]
    public void Crear_ConZonaHorariaMuyLarga_LanzaArgumentException()
    {
        // Arrange
        var zona = new string('a', Estudio.MaxZonaHoraria + 1);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Estudio.Crear("Test", "starter", zonaHoraria: zona));
        Assert.Contains($"no puede exceder {Estudio.MaxZonaHoraria} caracteres", ex.Message);
    }
}
