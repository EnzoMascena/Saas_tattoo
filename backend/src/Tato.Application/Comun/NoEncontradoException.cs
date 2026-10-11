namespace Tato.Application.Comun;

/// <summary>
/// Lo que se pidió no existe, o es de otro estudio (D-42): las dos cosas se responden igual, para no revelar
/// qué Ids existen en otros estudios.
/// </summary>
public sealed class NoEncontradoException : Exception
{
    public NoEncontradoException(string mensaje)
        : base(mensaje)
    {
    }
}
