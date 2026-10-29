using Tato.Application.Portfolio;

namespace Tato.Api.Portfolio;

/// <summary>
/// Registra los casos de uso del módulo Portfolio (doc 06, 3.2) y la configuración del sitio público.
/// </summary>
public static class PortfolioExtensions
{
    public static IServiceCollection AddPortfolio(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<CrearPublicacion>();
        services.AddScoped<EditarPublicacion>();
        services.AddScoped<AccionesPublicacion>();
        services.AddScoped<SubirFoto>();
        services.AddScoped<LeerFoto>();
        services.AddScoped<AccionesFotos>();
        services.AddScoped<CrearPagina>();
        services.AddScoped<AccionesPagina>();
        services.AddScoped<MisPaginas>();
        services.AddScoped<SubirFotoPagina>();
        services.AddScoped<LeerFotoDelSitio>();

        // Con una configuración imposible, la API no levanta (D-34).
        services.AddOptions<OpcionesSitio>()
            .Bind(configuration.GetSection(OpcionesSitio.Seccion))
            .Validate(
                o => o.SegundosCacheDatos > 0 && o.SegundosCacheFotos > 0,
                "Los tiempos de caché del sitio tienen que ser mayores a 0.")
            .Validate(o => o.LargoExtracto > 0, "El largo del extracto tiene que ser mayor a 0.")
            .ValidateOnStart();

        return services;
    }
}
