using Tato.Api.Comun;
using Tato.Application.Portfolio;
using Tato.Domain.Comun;

namespace Tato.IntegrationTests.Api;

/// <summary>
/// Qué respuesta recibe el panel ante cada tipo de error.
/// </summary>
public sealed class ManejadorErroresTests
{
    [Fact]
    public void Traducir_ReglaDeNegocio_Devuelve422ConElCodigoDeLaRegla()
    {
        // Act
        var problema = ManejadorErrores.Traducir(
            new ReglaDeNegocioException("RN-BLG-03", "Falta el consentimiento del cliente."));

        // Assert
        Assert.Equal(422, problema.Status);
        Assert.Equal("Falta el consentimiento del cliente.", problema.Detail);
        Assert.Equal("RN-BLG-03", problema.Extensions["regla"]);
    }

    [Fact]
    public void Traducir_FotoInvalida_Devuelve400ConElMensaje()
    {
        var problema = ManejadorErrores.Traducir(new FotoInvalidaException("El archivo no es una imagen."));

        Assert.Equal(400, problema.Status);
        Assert.Equal("El archivo no es una imagen.", problema.Detail);
    }

    [Fact]
    public void Traducir_ArgumentException_Devuelve400SinElNombreDelParametro()
    {
        var problema = ManejadorErrores.Traducir(
            new ArgumentException("El título no puede estar vacío.", "Titulo"));

        Assert.Equal(400, problema.Status);
        Assert.Equal("El título no puede estar vacío.", problema.Detail);
    }

    [Fact]
    public void Traducir_ErrorInesperado_Devuelve500SinDetalles()
    {
        // El mensaje interno podría tener datos sensibles (una consulta, una ruta): no se muestra.
        var problema = ManejadorErrores.Traducir(
            new InvalidOperationException("Connection string 'Server=...;Password=...' inválida"));

        Assert.Equal(500, problema.Status);
        Assert.Null(problema.Detail);
    }
}
