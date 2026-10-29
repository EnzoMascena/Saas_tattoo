namespace Tato.Application.Portfolio;

/// <summary>
/// Una foto con las medidas de su versión de detalle, para reservar el espacio antes de que cargue. La imagen se pide
/// aparte, por su Id y el tamaño que se quiera.
/// </summary>
public sealed record FotoConMedidas(Guid Id, int Ancho, int Alto);
