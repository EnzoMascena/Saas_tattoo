namespace Tato.Application.Comun;

/// <summary>
/// El usuario tiene sesión, pero su rol no le permite esta operación (por ejemplo, un Tatuador que intenta
/// publicar en la página del estudio, D-51). El mensaje se le muestra, así que dice qué no puede hacer.
/// </summary>
public sealed class AccesoDenegadoException : Exception
{
    public AccesoDenegadoException(string mensaje)
        : base(mensaje)
    {
    }
}
