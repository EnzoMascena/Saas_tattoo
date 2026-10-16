namespace Tato.Domain.Tests.Estudios;

using Tato.Domain.Estudios;
using Xunit;

public class EstiloTests
{
    [Fact]
    public void Crear_ConDatosValidos_DevuelveEstiloActivo()
    {
        // Arrange
        var estudioId = Guid.NewGuid();

        // Act
        var estilo = Estilo.Crear(estudioId, "  Fineline  ");

        // Assert
        Assert.NotEqual(Guid.Empty, estilo.Id);
        Assert.Equal(estudioId, estilo.EstudioId);
        Assert.Equal("Fineline", estilo.Nombre);
        Assert.True(estilo.Activo);
    }

    [Fact]
    public void Crear_ConEstudioVacio_LanzaArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => Estilo.Crear(Guid.Empty, "Fineline"));
        Assert.Contains("EstudioId no puede estar vacío", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConNombreVacio_LanzaArgumentException(string nombre)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => Estilo.Crear(Guid.NewGuid(), nombre));
        Assert.Contains("no puede estar vacío", ex.Message);
    }

    [Fact]
    public void Crear_ConNombreMayorA50Caracteres_LanzaArgumentException()
    {
        // Arrange
        var nombreLargo = new string('a', 51);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => Estilo.Crear(Guid.NewGuid(), nombreLargo));
        Assert.Contains("no puede exceder 50 caracteres", ex.Message);
    }

    [Fact]
    public void Renombrar_ConNombreValido_CambiaElNombre()
    {
        // Arrange
        var estilo = Estilo.Crear(Guid.NewGuid(), "Fine line");

        // Act
        estilo.Renombrar("Fineline");

        // Assert
        Assert.Equal("Fineline", estilo.Nombre);
    }

    [Fact]
    public void Renombrar_ConNombreVacio_LanzaArgumentExceptionYNoCambia()
    {
        // Arrange
        var estilo = Estilo.Crear(Guid.NewGuid(), "Fineline");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => estilo.Renombrar(" "));
        Assert.Equal("Fineline", estilo.Nombre);
    }

    [Fact]
    public void Desactivar_EstiloActivo_LoDejaInactivo()
    {
        // Arrange
        var estilo = Estilo.Crear(Guid.NewGuid(), "Fineline");

        // Act
        estilo.Desactivar();

        // Assert
        Assert.False(estilo.Activo);
    }

    [Fact]
    public void Activar_EstiloInactivo_LoVuelveAActivar()
    {
        // Arrange
        var estilo = Estilo.Crear(Guid.NewGuid(), "Fineline");
        estilo.Desactivar();

        // Act
        estilo.Activar();

        // Assert
        Assert.True(estilo.Activo);
    }
}
