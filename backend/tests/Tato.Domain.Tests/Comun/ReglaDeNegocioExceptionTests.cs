namespace Tato.Domain.Tests.Comun;

using Tato.Domain.Comun;
using Xunit;

public class ReglaDeNegocioExceptionTests
{
    [Fact]
    public void Crear_ConReglaYMensaje_LosGuarda()
    {
        // Act
        var ex = new ReglaDeNegocioException("RN-BLG-03", "Falta el consentimiento.");

        // Assert
        Assert.Equal("RN-BLG-03", ex.Regla);
        Assert.Equal("Falta el consentimiento.", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Crear_SinRegla_LanzaArgumentException(string regla)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new ReglaDeNegocioException(regla, "Mensaje"));
    }
}
