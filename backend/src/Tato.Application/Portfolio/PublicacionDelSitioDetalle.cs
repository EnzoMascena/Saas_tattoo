using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// El detalle de una publicación (doc 07, 7.3): las fotos en orden, el título, el texto y la fecha.
/// </summary>
public sealed record PublicacionDelSitioDetalle(
    Guid Id,
    TipoPublicacion Tipo,
    string Titulo,
    string? Descripcion,
    DateTimeOffset PublicadaEn,
    IReadOnlyList<FotoConMedidas> Fotos);
