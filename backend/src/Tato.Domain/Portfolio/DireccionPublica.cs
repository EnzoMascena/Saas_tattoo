using System.Text;

namespace Tato.Domain.Portfolio;

/// <summary>
/// La parte de la dirección web que identifica una página pública: «inkmortal» en plataforma.com/inkmortal
/// (RN-BLG-07). Es un objeto de valor: si existe, es válida. Que no la use otra página lo controla el caso de uso
/// contra la base, que es el único que puede ver las páginas de todos los estudios.
/// </summary>
public sealed record DireccionPublica
{
    public const int LargoMinimo = 3;
    public const int LargoMaximo = 40;

    /// <summary>
    /// Rutas de primer nivel de la propia plataforma: una página con alguna de estas direcciones las taparía.
    /// Si se suma una ruta nueva de primer nivel, se agrega acá.
    /// </summary>
    private static readonly HashSet<string> Reservadas =
    [
        "admin", "api", "app", "assets", "ayuda", "contacto", "estudio", "estudios", "favicon", "fotos",
        "login", "panel", "privacidad", "robots", "sitemap", "soporte", "static", "tato", "terminos", "www",
    ];

    private DireccionPublica(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    /// <summary>
    /// Arma la dirección con el nombre que eligió el estudio o el artista: en minúsculas, sin tildes, sin espacios
    /// ni signos. «Ink Mortal» queda «inkmortal» y «Ñandú Tattoo» queda «nandutattoo».
    /// </summary>
    public static DireccionPublica DesdeNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));

        var letras = new StringBuilder(LargoMaximo);

        // La forma D separa cada letra de su tilde («é» queda «e» más una tilde suelta), y la tilde se descarta
        // junto con los espacios y los signos.
        foreach (var caracter in nombre.Normalize(NormalizationForm.FormD).ToLowerInvariant())
        {
            if (char.IsAsciiLetterLower(caracter) || char.IsAsciiDigit(caracter))
                letras.Append(caracter);

            if (letras.Length == LargoMaximo)
                break;
        }

        return Validar(letras.ToString(), nameof(nombre));
    }

    /// <summary>
    /// Valida una dirección elegida a mano, por ejemplo al cambiarla desde el panel. Acepta letras minúsculas sin
    /// tildes, números y guiones entre medio: «ink-mortal» sí; «-ink», «ink--mortal» o «ink mortal», no.
    /// </summary>
    public static DireccionPublica Crear(string valor) => Validar(valor, nameof(valor));

    public override string ToString() => Valor;

    private static DireccionPublica Validar(string? valor, string nombreParametro)
    {
        var limpio = valor?.Trim().ToLowerInvariant() ?? string.Empty;

        if (limpio.Length is < LargoMinimo or > LargoMaximo)
            throw new ArgumentException(
                $"La dirección tiene que tener entre {LargoMinimo} y {LargoMaximo} letras o números.",
                nombreParametro);

        if (!TieneFormatoValido(limpio))
            throw new ArgumentException(
                "La dirección solo puede tener letras minúsculas sin tildes, números y guiones entre medio.",
                nombreParametro);

        if (Reservadas.Contains(limpio))
            throw new ArgumentException(
                $"La dirección «{limpio}» está reservada por la plataforma.", nombreParametro);

        return new DireccionPublica(limpio);
    }

    private static bool TieneFormatoValido(string valor) =>
        valor.All(c => char.IsAsciiLetterLower(c) || char.IsAsciiDigit(c) || c == '-')
        && valor[0] != '-'
        && valor[^1] != '-'
        && !valor.Contains("--", StringComparison.Ordinal);
}
