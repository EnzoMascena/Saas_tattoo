namespace Tato.Domain.Tests.Estudios;

using Tato.Domain.Estudios;
using Xunit;

/// <summary>
/// La zona horaria del estudio tiene que existir y ser un identificador IANA (D-43): con ella se calculan
/// reglas como «00:00 del día siguiente» (RN-TUR-06).
/// </summary>
public class EstudioZonaHorariaTests
{
    [Theory]
    [InlineData("Argentina")]
    [InlineData("Hora de Argentina")]
    [InlineData("America/Narnia")]
    public void Crear_ConZonaQueNoExiste_LanzaArgumentException(string zona)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => Estudio.Crear("Alkymia", "piloto", zonaHoraria: zona));
        Assert.Contains("no existe", ex.Message);
    }

    [Fact]
    public void Crear_ConIdentificadorDeWindows_LanzaArgumentException()
    {
        // El servidor corre en Linux, que usa IANA: el formato de Windows no sirve.
        Assert.Throws<ArgumentException>(() =>
            Estudio.Crear("Alkymia", "piloto", zonaHoraria: "Argentina Standard Time"));
    }

    [Fact]
    public void Crear_ConOtraZonaIana_LaGuarda()
    {
        // Act: por ejemplo, un estudio de Bolivia.
        var estudio = Estudio.Crear("Alkymia", "piloto", zonaHoraria: "America/La_Paz");

        // Assert
        Assert.Equal("America/La_Paz", estudio.ZonaHoraria);
    }
}
