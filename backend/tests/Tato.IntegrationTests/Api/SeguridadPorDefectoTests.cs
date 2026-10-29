using System.Net;
using System.Net.Http.Json;

namespace Tato.IntegrationTests.Api;

/// <summary>
/// Lo que la API exige por defecto, por HTTP (doc 06, 7.1): sesión en todo lo que no es público y el token
/// antiforgery en todo pedido que modifica datos.
/// </summary>
public sealed class SeguridadPorDefectoTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _api;

    public SeguridadPorDefectoTests(ApiFixture api)
    {
        _api = api;
    }

    [Fact]
    public async Task GetDelPanel_SinSesion_Responde401()
    {
        using var cliente = _api.CrearCliente();

        var respuesta = await cliente.GetAsync("/api/publicaciones");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Post_SinTokenAntiforgery_Responde400()
    {
        using var cliente = _api.CrearCliente();

        var respuesta = await cliente.PostAsJsonAsync("/api/auth/login", new { email = "a@b.c", password = "x" });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("antiforgery", await respuesta.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }
}
