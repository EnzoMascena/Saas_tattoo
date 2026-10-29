using Tato.Application.Estudios;

namespace Tato.Api.Estudios;

/// <summary>
/// Registra los servicios del módulo Estudios e identidad (doc 06, 3.2).
/// </summary>
public static class EstudiosExtensions
{
    public static IServiceCollection AddEstudios(this IServiceCollection services)
    {
        // El estudio de cada pedido sale de la cookie de sesión (D-42).
        services.AddHttpContextAccessor();
        services.AddScoped<IEstudioActual, EstudioActualDesdeSesion>();
        services.AddScoped<IUsuarioActual, UsuarioActualDesdeSesion>();

        services.AddScoped<AltaEstudio>();
        services.AddScoped<AltaUsuario>();

        return services;
    }
}
