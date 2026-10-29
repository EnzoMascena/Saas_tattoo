namespace Tato.Application.Portfolio;

/// <summary>
/// Una publicación en la grilla de Trabajos o en la lista del Blog (doc 07, 7.2).
/// </summary>
/// <param name="Titulo">También sirve de texto alternativo de la portada.</param>
/// <param name="Extracto">El comienzo del texto, solo en las del Blog.</param>
public sealed record PublicacionDelSitio(
    Guid Id,
    string Titulo,
    DateTimeOffset PublicadaEn,
    string? Extracto,
    FotoConMedidas Portada);
