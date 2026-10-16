using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Todo lo de una publicación, para la pantalla de edición del panel.
/// </summary>
/// <param name="Version">
/// Versión de la fila (rowversion) en base64. Al guardar cambios, el panel la devuelve: si alguien modificó la
/// publicación mientras tanto, la versión ya no coincide y el cambio se rechaza en lugar de pisar el otro.
/// </param>
public sealed record PublicacionDetalle(
    Guid Id,
    string Titulo,
    string? Descripcion,
    DateOnly? FechaTrabajo,
    Guid? EstiloId,
    Guid? ZonaCuerpoId,
    Guid? TatuadorId,
    bool Destacada,
    EstadoPublicacion Estado,
    bool TieneConsentimiento,
    DateTimeOffset? PublicadaEn,
    DateTimeOffset ActualizadaEn,
    string Version,
    IReadOnlyList<FotoDePublicacion> Fotos);
