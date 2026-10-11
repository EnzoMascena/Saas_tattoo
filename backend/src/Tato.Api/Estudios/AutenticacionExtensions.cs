using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Tato.Infrastructure.Estudios;
using Tato.Infrastructure.Persistencia;

namespace Tato.Api.Estudios;

/// <summary>
/// Registro de la autenticación (doc 06, 7.1 y 7.2): Identity con cookie, permisos
/// cerrados por defecto y límite de intentos en el login.
/// </summary>
public static class AutenticacionExtensions
{
    /// <summary>
    /// Nombre de la política de límite de intentos del login.
    /// </summary>
    public const string PoliticaLogin = "login";

    /// <summary>
    /// Encabezado en el que el panel manda el token antiforgery en cada pedido que modifica datos.
    /// </summary>
    public const string EncabezadoAntiforgery = "X-XSRF-TOKEN";

    public static IServiceCollection AddAutenticacion(this IServiceCollection services)
    {
        services.AddIdentityCore<CuentaUsuario>(opciones =>
            {
                // OWASP ASVS nivel 2 (doc 06, 7.5): al menos 12 caracteres y sin reglas de composición.
                opciones.Password.RequiredLength = 12;
                opciones.Password.RequireDigit = false;
                opciones.Password.RequireLowercase = false;
                opciones.Password.RequireUppercase = false;
                opciones.Password.RequireNonAlphanumeric = false;

                // 5 intentos fallidos bloquean la cuenta 15 minutos.
                opciones.Lockout.MaxFailedAccessAttempts = 5;
                opciones.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                opciones.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<TatoDbContext>()
            .AddSignInManager()
            .AddClaimsPrincipalFactory<FabricaClaimsCuenta>();

        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();

        // Va después de AddIdentityCookies, que es la que crea la cookie de la aplicación.
        services.ConfigureApplicationCookie(opciones =>
        {
            // __Host-: el navegador solo la acepta por HTTPS, para la raíz y sin compartirla con subdominios.
            opciones.Cookie.Name = "__Host-tato-sesion";
            opciones.Cookie.HttpOnly = true;
            opciones.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            opciones.Cookie.SameSite = SameSiteMode.Strict;

            // Vence tras 7 días sin uso; cada pedido la renueva (D-39).
            opciones.ExpireTimeSpan = TimeSpan.FromDays(7);
            opciones.SlidingExpiration = true;

            // Es una API para una SPA: sin sesión responde 401 y sin permiso 403, nunca redirige.
            // Los eventos se asignan de a uno: reemplazar Events entero desactiva la validación del security stamp.
            opciones.Events.OnRedirectToLogin = contexto =>
            {
                contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            opciones.Events.OnRedirectToAccessDenied = contexto =>
            {
                contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        // Antiforgery (doc 06, 7.1): todo pedido que modifica datos tiene que traer, además de la cookie de sesión,
        // un token en un encabezado. Otra página puede hacer que el navegador mande la cookie, pero no puede leer
        // el token, así que no puede falsificar un pedido. El token lo entrega GET /api/auth/antiforgery.
        services.AddAntiforgery(opciones =>
        {
            opciones.HeaderName = EncabezadoAntiforgery;
            opciones.Cookie.Name = "__Host-tato-af";
            opciones.Cookie.HttpOnly = true;
            opciones.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            opciones.Cookie.SameSite = SameSiteMode.Strict;
        });

        // Seguro por defecto (doc 06, 1): todo endpoint pide sesión salvo que diga [AllowAnonymous].
        services.AddAuthorization(opciones =>
            opciones.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        services.AddRateLimiter(opciones =>
        {
            opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Hasta 5 intentos de login por minuto por IP (doc 06, 7.1); se suma al bloqueo de la cuenta.
            // Detrás de Cloudflare la IP real llega en un encabezado: se configura en el despliegue (paso 4).
            opciones.AddPolicy(PoliticaLogin, contexto =>
                RateLimitPartition.GetFixedWindowLimiter(
                    contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                    }));
        });

        return services;
    }
}
