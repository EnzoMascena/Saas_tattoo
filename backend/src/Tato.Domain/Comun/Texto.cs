namespace Tato.Domain.Comun;

/// <summary>
/// Validaciones de texto que repiten todas las entidades: obligatorio u opcional, recortado y con largo máximo.
/// </summary>
internal static class Texto
{
    /// <summary>
    /// Valida un texto obligatorio y lo devuelve sin espacios en los extremos.
    /// </summary>
    public static string Obligatorio(string? valor, int largoMaximo, string descripcion, string nombreParametro)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException($"{descripcion} no puede estar vacío.", nombreParametro);

        var limpio = valor.Trim();

        if (limpio.Length > largoMaximo)
            throw new ArgumentException(
                $"{descripcion} no puede exceder {largoMaximo} caracteres.", nombreParametro);

        return limpio;
    }

    /// <summary>
    /// Valida un texto opcional. Vacío o con solo espacios se guarda como null, para que «no tiene» se guarde
    /// de una sola forma.
    /// </summary>
    public static string? Opcional(string? valor, int largoMaximo, string descripcion, string nombreParametro)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        var limpio = valor.Trim();

        if (limpio.Length > largoMaximo)
            throw new ArgumentException(
                $"{descripcion} no puede exceder {largoMaximo} caracteres.", nombreParametro);

        return limpio;
    }
}
