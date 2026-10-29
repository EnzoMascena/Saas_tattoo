using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

    [Fact]
    public async Task Pagina_Visible_Responde200SinSesionConCacheDeUnMinuto()
    {
        // Arrange
        var duenio = await PanelAsync();
        var (_, direccion) = await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        using var cliente = _api.CrearCliente();

        // Act
        var respuesta = await cliente.GetAsync($"/api/publico/paginas/{direccion}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("public, max-age=60", respuesta.Headers.CacheControl?.ToString());
        var pagina = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(direccion.Valor, pagina.GetProperty("direccion").GetString());
    }

    [Theory]
    [InlineData("api")]
    [InlineData("--mal-formada")]
    [InlineData("no-existe-ninguna-con-esta")]
    public async Task Pagina_ReservadaMalFormadaOQueNoExiste_Responde404(string direccion)
    {
        using var cliente = _api.CrearCliente();

        var respuesta = await cliente.GetAsync($"/api/publico/paginas/{direccion}");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Pagina_Oculta_Responde404ConSusPublicaciones()
    {
        // Arrange: la página tenía una publicación publicada cuando se ocultó.
        var duenio = await PanelAsync();
        var (id, direccion) = await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        var publicacion = await duenio.PublicarNuevaAsync(new DatosPublicacion("Publicada"));
        await duenio.OcultarPaginaAsync(id, (await duenio.ObtenerPaginaAsync(id)).Version);
        using var cliente = _api.CrearCliente();

        // Act & Assert
        foreach (var ruta in new[]
        {
            $"/api/publico/paginas/{direccion}",
            $"/api/publico/paginas/{direccion}/publicaciones",
            $"/api/publico/paginas/{direccion}/publicaciones/{publicacion}",
        })
            Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync(ruta)).StatusCode);
    }

    [Fact]
    public async Task ElJson_NoTraeDatosInternosYLosEnumsVanComoTexto()
    {
        // Arrange: una publicación con datos internos cargados, como la fecha del trabajo y el consentimiento.
        var duenio = await PanelAsync();
        var (_, direccion) = await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        var id = await duenio.PublicarNuevaAsync(
            new DatosPublicacion("Espiral", Descripcion: "Dos sesiones.", FechaTrabajo: new DateOnly(2026, 10, 1)));
        using var cliente = _api.CrearCliente();

        // Act
        var respuestas = new[]
        {
            await cliente.GetStringAsync($"/api/publico/paginas/{direccion}"),
            await cliente.GetStringAsync($"/api/publico/paginas/{direccion}/publicaciones"),
            await cliente.GetStringAsync($"/api/publico/paginas/{direccion}/publicaciones/{id}"),
        };

        // Assert: nada de lo que el sitio no muestra (RN-BLG-02).
        string[] internos =
        [
            "estudioId", "tatuadorId", "usuarioId", "creadaPor", "consentimiento", "estiloId", "zonaCuerpoId",
            "fechaTrabajo", "estado", "version", "visible", "destacada",
        ];

        foreach (var json in respuestas)
        {
            Assert.All(internos, campo => Assert.DoesNotContain($"\"{campo}", json, StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(duenio.EstudioId.ToString(), json, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("\"tipo\":\"Trabajo\"", respuestas[2], StringComparison.Ordinal);
    }

    [Fact]
    public async Task Cache_GuardaLaRespuestaAunqueCambieLaBase()
    {
        // Arrange: la primera visita queda en la caché.
        var duenio = await PanelAsync();
        var (id, direccion) = await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        var ruta = $"/api/publico/paginas/{direccion}";
        using var visitante = _api.CrearCliente();
        Assert.Equal(HttpStatusCode.OK, (await visitante.GetAsync(ruta)).StatusCode);

        // Act: se oculta directo en la base, sin pasar por la API, así que nada vacía la caché.
        await duenio.OcultarPaginaAsync(id, (await duenio.ObtenerPaginaAsync(id)).Version);

        // Assert: la segunda visita sale de la caché.
        Assert.Equal(HttpStatusCode.OK, (await visitante.GetAsync(ruta)).StatusCode);
    }

    [Fact]
    public async Task Cache_UnCambioDesdeElPanelLaVacia()
    {
        // Arrange: un Dueño crea y muestra la página del estudio desde el panel, por HTTP.
        using var panel = await _api.IniciarSesionAsync();
        var creada = await panel.PostAsJsonAsync(
            "/api/paginas", new { delEstudio = true, nombre = NombreUnico("Alkimya") });
        Assert.Equal(HttpStatusCode.Created, creada.StatusCode);
        var id = (await creada.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var detalle = await panel.GetFromJsonAsync<JsonElement>($"/api/paginas/{id}");
        var mostrada = await panel.PostAsJsonAsync(
            $"/api/paginas/{id}/mostrar", new { version = detalle.GetProperty("version").GetString() });
        var version = (await mostrada.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("version").GetString();

        var ruta = $"/api/publico/paginas/{detalle.GetProperty("direccion").GetString()}";
        using var visitante = _api.CrearCliente();
        Assert.Equal(HttpStatusCode.OK, (await visitante.GetAsync(ruta)).StatusCode);

        // Act
        var ocultada = await panel.PostAsJsonAsync($"/api/paginas/{id}/ocultar", new { version });
        Assert.Equal(HttpStatusCode.OK, ocultada.StatusCode);

        // Assert: el sitio deja de mostrarla sin esperar a que venza la caché (D-57).
        Assert.Equal(HttpStatusCode.NotFound, (await visitante.GetAsync(ruta)).StatusCode);
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
