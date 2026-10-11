using Tato.Application.Portfolio;

namespace Tato.Api.Portfolio;

/// <summary>
/// Registra los casos de uso del módulo Portfolio (doc 06, 3.2).
/// </summary>
public static class PortfolioExtensions
{
    public static IServiceCollection AddPortfolio(this IServiceCollection services)
    {
        services.AddScoped<CrearPublicacion>();
        services.AddScoped<EditarPublicacion>();

        return services;
    }
}
