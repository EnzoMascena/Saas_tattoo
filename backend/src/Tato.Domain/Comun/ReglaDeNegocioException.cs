namespace Tato.Domain.Comun;

/// <summary>
/// Se lanza cuando una regla de negocio no permite la operación: por ejemplo, publicar un trabajo sin el
/// consentimiento del cliente (RN-BLG-03). Los datos pueden estar bien formados; lo que falla es la regla.
/// <para>
/// <see cref="ArgumentException"/> queda para los datos mal formados (un título vacío, un Id vacío), que la
/// validación de entrada tiene que frenar antes de llegar al dominio. La API muestra esta excepción como un
/// error con su mensaje y su <see cref="Regla"/>.
/// </para>
/// </summary>
public sealed class ReglaDeNegocioException : Exception
{
    public ReglaDeNegocioException(string regla, string mensaje)
        : base(mensaje)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(regla);
        Regla = regla;
    }

    /// <summary>
    /// Código de la regla que no se cumple, tal como figura en la documentación (ej: "RN-BLG-03").
    /// Le sirve al front para reaccionar a una regla puntual sin depender del texto del mensaje.
    /// </summary>
    public string Regla { get; }
}
