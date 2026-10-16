namespace Tato.Application.Portfolio;

/// <summary>
/// Resultado de subir una foto: la foto nueva y la versión nueva de la publicación, para seguir editando.
/// </summary>
public sealed record FotoSubida(FotoDePublicacion Foto, string Version);
