using Tato.Application.Comun;
using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Filtros de «Mis publicaciones». Sin estado, trae todas.
/// </summary>
public sealed record FiltroPublicaciones(EstadoPublicacion? Estado, Paginacion Paginacion);
