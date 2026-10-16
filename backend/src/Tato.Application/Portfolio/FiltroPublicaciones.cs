using Tato.Application.Comun;
using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Filtros de «Mis publicaciones». Sin estado ni tipo, trae todas.
/// </summary>
/// <param name="Tipo">Trabajo o Blog (D-50). Va al final y es opcional, así los usos que ya existen no cambian.</param>
public sealed record FiltroPublicaciones(
    EstadoPublicacion? Estado,
    Paginacion Paginacion,
    TipoPublicacion? Tipo = null);
