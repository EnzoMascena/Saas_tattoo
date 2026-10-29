namespace Tato.Application.Portfolio;

/// <summary>
/// Resultado de subir una foto de página: la foto nueva y la versión nueva de la página, para seguir editando.
/// </summary>
public sealed record FotoPaginaSubida(FotoConMedidas Foto, string Version);
