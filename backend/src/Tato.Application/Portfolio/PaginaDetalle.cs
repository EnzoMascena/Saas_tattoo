namespace Tato.Application.Portfolio;

/// <summary>
/// Todo lo de una página pública, para la pantalla «Mi página» del panel.
/// </summary>
/// <param name="TatuadorId">El artista de la página, o null si es la del estudio.</param>
/// <param name="Direccion">La ruta de la página en el sitio: plataforma.com/{Direccion} (RN-BLG-07).</param>
/// <param name="Foto">La foto o logo del encabezado, o null si no tiene (R-16).</param>
/// <param name="FotoSobre">La foto de «Sobre», o null si no tiene (R-16).</param>
/// <param name="Version">Versión de la fila en base64. Al guardar cambios, el panel la devuelve.</param>
public sealed record PaginaDetalle(
    Guid Id,
    bool EsDelEstudio,
    Guid? TatuadorId,
    string Direccion,
    string Nombre,
    string? Bio,
    string? Instagram,
    string? WhatsApp,
    string? Sobre,
    string? Ubicacion,
    string? Horarios,
    FotoConMedidas? Foto,
    FotoConMedidas? FotoSobre,
    bool Visible,
    DateTimeOffset ActualizadaEn,
    string Version);
