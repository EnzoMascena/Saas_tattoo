namespace Tato.Domain.Tests.Estudios;

using Tato.Domain.Estudios;
using Xunit;

/// <summary>
/// Las reglas del catálogo están en ElementoCatalogo y se prueban en EstiloTests.
/// Acá alcanza con comprobar que la zona las hereda.
/// </summary>
public class ZonaCuerpoTests
{
    [Fact]
    public void Crear_ConDatosValidos_DevuelveZonaActiva()
    {
        // Arrange
        var estudioId = Guid.NewGuid();

        // Act
        var zona = ZonaCuerpo.Crear(estudioId, " Antebrazo ");

        // Assert
        Assert.NotEqual(Guid.Empty, zona.Id);
        Assert.Equal(estudioId, zona.EstudioId);
        Assert.Equal("Antebrazo", zona.Nombre);
        Assert.True(zona.Activo);
    }

    [Fact]
    public void Crear_ConNombreVacio_LanzaArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ZonaCuerpo.Crear(Guid.NewGuid(), ""));
    }

    [Fact]
    public void Crear_ConEstudioVacio_LanzaArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ZonaCuerpo.Crear(Guid.Empty, "Antebrazo"));
    }
}
