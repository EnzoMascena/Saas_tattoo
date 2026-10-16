namespace Tato.Application.Portfolio;

/// <summary>
/// La foto subida no se puede usar: no es una imagen, es de un formato que no se acepta o supera los límites
/// de tamaño. El mensaje se le muestra a quien la subió, así que tiene que ser claro.
/// </summary>
public sealed class FotoInvalidaException : Exception
{
    public FotoInvalidaException(string mensaje)
        : base(mensaje)
    {
    }
}
