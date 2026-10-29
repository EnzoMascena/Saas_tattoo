namespace Tato.Application.Estudios;

/// <summary>
/// Resultado de un alta que puede fallar por las reglas de la cuenta, como una contraseña corta o un email repetido.
/// </summary>
public sealed record ResultadoAlta(bool Exito, IReadOnlyList<string> Errores)
{
    public static ResultadoAlta Exitoso() => new(true, []);

    public static ResultadoAlta Fallido(IEnumerable<string> errores) => new(false, errores.ToList());
}
