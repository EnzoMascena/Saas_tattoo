using System.Security.Claims;
using Tato.Application.Estudios;
using Tato.Domain.Estudios;

namespace Tato.Api.Estudios;

/// <summary>
/// Toma el Id y los roles del usuario de los claims que guarda la cookie al iniciar sesión.
/// </summary>
internal sealed class UsuarioActualDesdeSesion : IUsuarioActual
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioActualDesdeSesion(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Usuario => _httpContextAccessor.HttpContext?.User;

    public Guid? UsuarioId =>
        Guid.TryParse(Usuario?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var usuarioId)
            ? usuarioId
            : null;

    public bool EsDuenio => Usuario?.IsInRole(nameof(Rol.Duenio)) ?? false;

    public bool EsAdministrativo => Usuario?.IsInRole(nameof(Rol.Administrativo)) ?? false;
}
