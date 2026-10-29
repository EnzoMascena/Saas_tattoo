namespace Tato.Application.Portfolio;

/// <summary>
/// Una página tal como la ve un visitante del sitio (doc 07, 7): el encabezado, el «Sobre» y las páginas
/// relacionadas. Sin Ids internos ni datos que el sitio no muestra (RN-BLG-02).
/// </summary>
/// <param name="Ubicacion">Solo en la página del estudio.</param>
/// <param name="Horarios">Solo en la página del estudio.</param>
/// <param name="Estudio">En la página de un artista, la de su estudio, si está visible (doc 07, 7.4).</param>
/// <param name="Artistas">En la página del estudio, las de sus artistas que están visibles, por nombre.</param>
public sealed record PaginaDelSitio(
    string Direccion,
    bool EsDelEstudio,
    string Nombre,
    string? Bio,
    FotoConMedidas? Foto,
    string? Instagram,
    string? WhatsApp,
    string? Sobre,
    FotoConMedidas? FotoSobre,
    string? Ubicacion,
    string? Horarios,
    EnlacePagina? Estudio,
    IReadOnlyList<EnlacePagina> Artistas);
