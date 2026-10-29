using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Tato.Api.Estudios;
using Tato.Application.Comun;
using Tato.Application.Estudios;
using Tato.Domain.Estudios;
using Tato.IntegrationTests.Portfolio;

namespace Tato.IntegrationTests.Api;

/// <summary>
/// La API entera en memoria, contra la base de los tests: para probar lo que solo se ve por HTTP, como la sesión, el
/// antiforgery, la caché y los encabezados. Lo comparten todos los tests de una clase.
/// <para>
/// Los datos se cargan con el <see cref="Panel"/>, sobre la misma base y el mismo almacenamiento en memoria.
/// </para>
/// </summary>
public sealed class ApiFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _fabrica;

    public BaseDeDatosFixture BaseDeDatos { get; } = new();

    internal AlmacenamientoEnMemoria Almacenamiento { get; } = new();

    public async Task InitializeAsync()
    {
        await BaseDeDatos.InitializeAsync();

        _fabrica = new WebApplicationFactory<Program>().WithWebHostBuilder(web =>
        {
            // Testing y no Development: así no carga los user-secrets, que apuntan a la base de desarrollo.
            web.UseEnvironment("Testing");
            web.UseSetting("ConnectionStrings:Tato", BaseDeDatos.CadenaConexion);
            web.ConfigureTestServices(servicios =>
                servicios.AddSingleton<IAlmacenamientoArchivos>(Almacenamiento));
        });
    }

    public async Task DisposeAsync()
    {
        if (_fabrica is not null)
            await _fabrica.DisposeAsync();

        await BaseDeDatos.DisposeAsync();
    }

    /// <summary>
    /// Un cliente sin sesión, como un visitante. Por HTTPS: las cookies __Host- y el antiforgery lo exigen.
    /// </summary>
    public HttpClient CrearCliente() =>
        _fabrica!.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    /// <summary>
    /// Da de alta un estudio con un Dueño, como los comandos de consola, e inicia sesión por HTTP como el panel: pide
    /// el token antiforgery, inicia sesión y pide otro. Devuelve un cliente que manda la cookie y el token en cada
    /// pedido. Ojo: el login tiene un límite de 5 por minuto, y en los tests todos los pedidos llegan sin IP.
    /// </summary>
    public async Task<HttpClient> IniciarSesionAsync()
    {
        var email = $"{Guid.NewGuid():N}@tato.test";

        // Una contraseña al azar, que solo existe en esta base de tests.
        var contrasenia = Guid.NewGuid().ToString("N");

        using (var alcance = _fabrica!.Services.CreateScope())
        {
            var estudioId = await alcance.ServiceProvider.GetRequiredService<AltaEstudio>()
                .EjecutarAsync($"Estudio {Guid.NewGuid():N}", "starter", zonaHoraria: null);
            var alta = await alcance.ServiceProvider.GetRequiredService<AltaUsuario>()
                .EjecutarAsync(new SolicitudAltaUsuario(estudioId, email, "Dueño de prueba", Rol.Duenio), contrasenia);

            Assert.True(alta.Exito, string.Join(" ", alta.Errores));
        }

        var cliente = CrearCliente();
        await PedirTokenAsync(cliente);

        var login = await cliente.PostAsJsonAsync("/api/auth/login", new { email, password = contrasenia });
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

        // El token de antes de iniciar sesión ya no sirve.
        await PedirTokenAsync(cliente);

        return cliente;
    }

    private static async Task PedirTokenAsync(HttpClient cliente)
    {
        var respuesta = await cliente.GetFromJsonAsync<JsonElement>("/api/auth/antiforgery");

        cliente.DefaultRequestHeaders.Remove(AutenticacionExtensions.EncabezadoAntiforgery);
        cliente.DefaultRequestHeaders.Add(
            AutenticacionExtensions.EncabezadoAntiforgery, respuesta.GetProperty("token").GetString());
    }
}
