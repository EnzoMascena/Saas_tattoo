using System.Net;
using Tato.Domain.Portfolio;
using Tato.IntegrationTests.Portfolio;

namespace Tato.IntegrationTests.Api;

/// <summary>
/// Los endpoints del sitio público por HTTP, como los pide un visitante: sin sesión, con los encabezados de caché
/// (D-57) y con 404 para lo que no se ve. Qué se ve y qué no lo prueban en detalle ConsultasDelSitioTests y
/// FotosDelSitioTests.
/// </summary>
public sealed class SitioApiTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _api;

    public SitioApiTests(ApiFixture api)
    {
        _api = api;
    }

    [Fact]
    public async Task Foto_DeUnaPublicada_RespondeWebpConCacheDeUnaHora()
    {
        // Arrange
        var duenio = await PanelAsync();
        await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        var id = await duenio.PublicarNuevaAsync(new DatosPublicacion("Espiral"));
        var foto = (await duenio.ObtenerAsync(id)).Fotos[0].Id;
        using var cliente = _api.CrearCliente();

        // Act
        var respuesta = await cliente.GetAsync($"/api/publico/fotos/{foto}/galeria.webp");

        // Assert
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("image/webp", respuesta.Content.Headers.ContentType?.MediaType);
        Assert.Equal("public, max-age=3600", respuesta.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Foto_DeUnaOculta_Responde404()
    {
        // Arrange
        var duenio = await PanelAsync();
        await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        var id = await duenio.PublicarNuevaAsync(new DatosPublicacion("Oculta"));
        await duenio.OcultarAsync(id, (await duenio.ObtenerAsync(id)).Version);
        var foto = (await duenio.ObtenerAsync(id)).Fotos[0].Id;
        using var cliente = _api.CrearCliente();

        // Act
        var respuesta = await cliente.GetAsync($"/api/publico/fotos/{foto}/galeria.webp");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    /// <summary>
    /// Un Dueño de un estudio nuevo, que guarda las fotos donde las lee la API.
    /// </summary>
    private Task<Panel> PanelAsync() =>
        Panel.CrearAsync(_api.BaseDeDatos, UsuarioFalso.Duenio(), almacenamiento: _api.Almacenamiento);

    /// <summary>
    /// Todos los tests de la clase comparten la base: cada página necesita un nombre, y una dirección, únicos.
    /// </summary>
    private static string NombreUnico(string nombre) => $"{nombre} {Guid.NewGuid():N}";
}
