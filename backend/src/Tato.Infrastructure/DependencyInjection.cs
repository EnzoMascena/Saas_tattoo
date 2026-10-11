using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tato.Application.Comun;
using Tato.Application.Estudios;
using Tato.Application.Portfolio;
using Tato.Infrastructure.Comun;
using Tato.Infrastructure.Estudios;
using Tato.Infrastructure.Persistencia;
using Tato.Infrastructure.Portfolio;
using Tato.Infrastructure.Portfolio;

namespace Tato.Infrastructure;

/// <summary>
/// Registra los servicios de Infrastructure. La Api lo llama una sola vez desde Program.cs:
/// depende de Infrastructure solo para componer (doc 06, 3.1).
/// </summary>
public static class DependencyInjection
{
    private const string NombreCadenaConexion = "Tato";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Desarrollo: user-secrets. Producción: variable de entorno ConnectionStrings__Tato.
        // Nunca en el repositorio (doc 06, 7.3).
        var cadenaConexion = configuration.GetConnectionString(NombreCadenaConexion);

        // Si falta, mejor fallar al arrancar que en la primera consulta.
        if (string.IsNullOrWhiteSpace(cadenaConexion))
            throw new InvalidOperationException(
                $"Falta la cadena de conexión 'ConnectionStrings:{NombreCadenaConexion}'. " +
                "En desarrollo se carga con dotnet user-secrets en el proyecto Tato.Api.");

        services.AddDbContext<TatoDbContext>(options => options.UseSqlServer(cadenaConexion));

        // Adaptadores de los puertos de Application (doc 06, 6.1).
        services.AddScoped<IRegistroEstudios, RegistroEstudios>();
        
        services.AddScoped<IConsultasPublicaciones, ConsultasPublicaciones>();

        AgregarArchivosYFotos(services, configuration);

        return services;
    }

    /// <summary>
    /// Almacenamiento y procesamiento de fotos, con sus opciones (patrón Options). Las opciones se validan al
    /// arrancar: con una configuración imposible la API no levanta, en lugar de fallar con la primera foto.
    /// </summary>
    private static void AgregarArchivosYFotos(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OpcionesAlmacenamientoDisco>()
            .Bind(configuration.GetSection(OpcionesAlmacenamientoDisco.Seccion))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.CarpetaRaiz),
                $"Falta '{OpcionesAlmacenamientoDisco.Seccion}:CarpetaRaiz'.")
            // Una ruta relativa se toma desde la carpeta de la Api, no desde donde se corrió el comando.
            .PostConfigure<IHostEnvironment>((o, entorno) =>
                o.CarpetaRaiz = Path.GetFullPath(o.CarpetaRaiz, entorno.ContentRootPath))
            .ValidateOnStart();

        services.AddOptions<OpcionesFotos>()
            .Bind(configuration.GetSection(OpcionesFotos.Seccion))
            .Validate(o => o.MaxBytes > 0 && o.MaxPixeles > 0, "Los máximos de las fotos tienen que ser mayores a 0.")
            .Validate(
                o => o.LadoMiniatura > 0 && o.LadoMiniatura <= o.LadoGaleria && o.LadoGaleria <= o.LadoDetalle,
                "Los lados de las fotos tienen que cumplir 0 < miniatura <= galería <= detalle.")
            .Validate(o => o.CalidadWebp is >= 1 and <= 100, "La calidad de WebP tiene que estar entre 1 y 100.")
            .ValidateOnStart();

        // Singleton: no guardan estado entre pedidos, así que una sola instancia sirve para todos.
        services.AddSingleton<IAlmacenamientoArchivos, AlmacenamientoEnDisco>();
        services.AddSingleton<IProcesadorFotos, ProcesadorFotosNetVips>();
    }
}
