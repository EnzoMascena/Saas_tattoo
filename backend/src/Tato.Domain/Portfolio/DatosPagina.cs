namespace Tato.Domain.Portfolio;

/// <summary>
/// Datos que se editan de una página pública desde «Mi página». La dirección no está: se cambia aparte y a
/// propósito, para no romper los links ya compartidos (RN-BLG-07).
/// </summary>
/// <param name="Nombre">El nombre visible: el del estudio o el artístico. Obligatorio.</param>
/// <param name="Bio">La bio corta del encabezado, como en Instagram.</param>
/// <param name="Instagram">El usuario de Instagram, con o sin @.</param>
/// <param name="WhatsApp">El número de WhatsApp en formato internacional, por ejemplo +54 9 221 555-0000.</param>
/// <param name="Sobre">El texto de la pestaña «Sobre».</param>
/// <param name="Ubicacion">Dónde queda el local. Solo en la página del estudio.</param>
/// <param name="Horarios">Los horarios de atención. Solo en la página del estudio.</param>
public sealed record DatosPagina(
    string Nombre,
    string? Bio = null,
    string? Instagram = null,
    string? WhatsApp = null,
    string? Sobre = null,
    string? Ubicacion = null,
    string? Horarios = null);
