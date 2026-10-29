using Tato.Application.Comun;
using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Qué publicaciones de una página se piden: de qué tipo, qué página de la lista y hasta cuántos caracteres del texto
/// van en el extracto de las del Blog.
/// </summary>
public sealed record FiltroPublicacionesDelSitio(TipoPublicacion Tipo, Paginacion Paginacion, int LargoExtracto);
