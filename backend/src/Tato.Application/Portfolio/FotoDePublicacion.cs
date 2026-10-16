namespace Tato.Application.Portfolio;

/// <summary>
/// Una foto de una publicación, en su orden. La imagen se pide aparte, por su Id y el tamaño que se quiera.
/// </summary>
public sealed record FotoDePublicacion(Guid Id, int Orden, int Ancho, int Alto);
