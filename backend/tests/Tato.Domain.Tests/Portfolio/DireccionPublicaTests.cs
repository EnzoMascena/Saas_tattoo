namespace Tato.Domain.Tests.Portfolio;

using Tato.Domain.Portfolio;
using Xunit;

/// <summary>
/// La dirección de una página pública (RN-BLG-07): cómo se arma desde el nombre y qué se acepta a mano.
/// </summary>
public class DireccionPublicaTests
{
    [Theory]
    [InlineData("Ink Mortal", "inkmortal")]
    [InlineData("Belén Mascena", "belenmascena")]
    [InlineData("Ñandú Tattoo!", "nandutattoo")]
    [InlineData("EMA DOT", "emadot")]
    public void DesdeNombre_SacaMayusculasTildesEspaciosYSignos(string nombre, string esperada)
    {
        Assert.Equal(esperada, DireccionPublica.DesdeNombre(nombre).Valor);
    }

    [Fact]
    public void DesdeNombre_ConUnNombreLargo_LaCortaEnElMaximo()
    {
        var direccion = DireccionPublica.DesdeNombre(new string('a', 60));

        Assert.Equal(DireccionPublica.LargoMaximo, direccion.Valor.Length);
    }

    [Theory]
    [InlineData("Ab")]
    [InlineData("!!!")]
    public void DesdeNombre_SinLetrasSuficientes_LanzaArgumentException(string nombre)
    {
        Assert.Throws<ArgumentException>(() => DireccionPublica.DesdeNombre(nombre));
    }

    [Theory]
    [InlineData("ink-mortal")]
    [InlineData("Ink-Mortal")]
    public void Crear_ConGuionesEntreMedio_LaAceptaEnMinusculas(string valor)
    {
        Assert.Equal("ink-mortal", DireccionPublica.Crear(valor).Valor);
    }

    [Theory]
    [InlineData("-ink")]
    [InlineData("ink-")]
    [InlineData("ink--mortal")]
    [InlineData("ink mortal")]
    [InlineData("inkmortál")]
    [InlineData("ab")]
    public void Crear_ConFormatoInvalido_LanzaArgumentException(string valor)
    {
        Assert.Throws<ArgumentException>(() => DireccionPublica.Crear(valor));
    }

    [Theory]
    [InlineData("panel")]
    [InlineData("API")]
    public void Crear_UnaRutaDeLaPlataforma_LanzaArgumentException(string valor)
    {
        var ex = Assert.Throws<ArgumentException>(() => DireccionPublica.Crear(valor));

        Assert.Contains("reservada", ex.Message);
    }

    [Fact]
    public void DosDireccionesConElMismoValor_SonIguales()
    {
        // Es un objeto de valor: se compara por lo que contiene, no por ser la misma instancia.
        Assert.Equal(DireccionPublica.Crear("inkmortal"), DireccionPublica.DesdeNombre("Ink Mortal"));
    }
}
