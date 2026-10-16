using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Una fila de «Mis publicaciones»: lo justo para la lista, sin descripción ni fotos.
/// </summary>
/// <param name="Tipo">Trabajo o Blog (D-50).</param>
/// <param name="PortadaFotoId">La primera foto, para mostrar la miniatura. Null si todavía no tiene fotos.</param>
public sealed record PublicacionResumen(
    Guid Id,
    string Titulo,
    TipoPublicacion Tipo,
    EstadoPublicacion Estado,
    bool Destacada,
    DateOnly? FechaTrabajo,
    DateTimeOffset ActualizadaEn,
    Guid? PortadaFotoId,
    int CantidadFotos);
