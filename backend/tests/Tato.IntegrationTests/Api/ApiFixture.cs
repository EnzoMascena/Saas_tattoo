using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Tato.Application.Comun;
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
}
