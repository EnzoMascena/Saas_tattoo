using Tato.Application.Estudios;

namespace Tato.Api.Estudios;

/// <summary>
/// Toma el estudio del claim que se guardó en la cookie al iniciar sesión (D-42).
/// </summary>
internal sealed class EstudioActualDesdeSesion : IEstudioActual
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EstudioActualDesdeSesion(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? EstudioId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimsTato.EstudioId)?.Value, out var estudioId)
            ? estudioId
            : null;
}
