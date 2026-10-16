namespace Tato.Domain.Comun;

/// <summary>
/// Validación de las fechas que recibe el dominio (D-43). El dominio no lee el reloj: le pasan «ahora»
/// desde el caso de uso, que lo obtiene de TimeProvider. Así los tests pueden fijar la hora.
/// </summary>
internal static class FechaUtc
{
    /// <summary>
    /// Devuelve la fecha si está en UTC. Cualquier otro desfase es un error de quien llama: el dominio guarda
    /// todo en UTC y la zona del estudio se aplica solo al mostrar.
    /// </summary>
    public static DateTimeOffset Validar(DateTimeOffset fecha, string nombreParametro)
    {
        if (fecha.Offset != TimeSpan.Zero)
            throw new ArgumentException("La fecha tiene que estar en UTC (D-43).", nombreParametro);

        return fecha;
    }
}
