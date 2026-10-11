namespace Tato.Application.Comun;

/// <summary>
/// El usuario tiene sesión, pero su rol no le permite esta operación (por ejemplo, un Tatuador que intenta
/// publicar, P-21). El mensaje se le muestra, así que dice qué no puede hacer.
/// </summary>
public sealed class AccesoDenegadoException : Exception
{
    public AccesoDenegadoException(string mensaje)
        : base(mensaje)
    {
    }
}
