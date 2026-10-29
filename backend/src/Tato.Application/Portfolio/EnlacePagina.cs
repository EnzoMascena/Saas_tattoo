namespace Tato.Application.Portfolio;

/// <summary>
/// El link a otra página: su dirección, su nombre y su foto o logo.
/// </summary>
public sealed record EnlacePagina(string Direccion, string Nombre, FotoConMedidas? Foto);
