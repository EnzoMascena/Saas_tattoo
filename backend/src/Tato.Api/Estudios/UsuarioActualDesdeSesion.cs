using System.Security.Claims;
using Tato.Application.Estudios;

namespace Tato.Api.Estudios;

/// <summary>
/// Toma el Id del usuario del claim de identificador que guarda la cookie al iniciar sesión.
/// </summary>
internal sealed class UsuarioActualDesdeSesion : IUsuarioActual
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioActualDesdeSesion(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UsuarioId =>
        Guid.TryParse(
            _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            out var usuarioId)
            ? usuarioId
            : null;
}
