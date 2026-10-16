using Tato.Domain.Comun;

namespace Tato.Domain.Portfolio;

/// <summary>
/// La página pública de un estudio o de un artista (D-49): su dirección, su encabezado y su «Sobre». Las
/// publicaciones no viven acá: cada una dice de qué página es con su TatuadorId (RN-BLG-12).
/// <para>
/// Arranca oculta: no aparece en el sitio hasta que alguien la completa y la muestra a propósito.
/// </para>
/// </summary>
public sealed class PaginaPublica : IPerteneceAEstudio
{
    public const int MaxNombre = 100;
    public const int MaxBio = 150;
    public const int MaxInstagram = 30;

    /// <summary>
    /// Formato internacional (E.164): el signo + y hasta 15 dígitos.
    /// </summary>
    public const int MaxWhatsApp = 16;

    public const int MaxSobre = 2200;
    public const int MaxUbicacion = 200;
    public const int MaxHorarios = 300;

    private PaginaPublica(
        Guid id,
        Guid estudioId,
        Guid? tatuadorId,
        DireccionPublica direccion,
        DateTimeOffset creadaEn)
    {
        Id = id;
        EstudioId = estudioId;
        TatuadorId = tatuadorId;
        Direccion = direccion;
        Nombre = string.Empty;
        CreadaEn = creadaEn;
        ActualizadaEn = creadaEn;
    }

    public Guid Id { get; }

    /// <summary>
    /// Estudio al que pertenece (D-42).
    /// </summary>
    public Guid EstudioId { get; }

    /// <summary>
    /// Perfil del artista de la página, o null si es la página del estudio. No cambia: una página no pasa de un
    /// artista a otro.
    /// </summary>
    public Guid? TatuadorId { get; }

    public bool EsDelEstudio => TatuadorId is null;

    /// <summary>
    /// Dónde se ve: plataforma.com/{Direccion}. Sale del nombre al crear la página y después solo cambia a
    /// propósito (RN-BLG-07).
    /// </summary>
    public DireccionPublica Direccion { get; private set; }

    public string Nombre { get; private set; }

    public string? Bio { get; private set; }

    /// <summary>
    /// Usuario de Instagram, sin @.
    /// </summary>
    public string? Instagram { get; private set; }

    /// <summary>
    /// Número en formato internacional, sin espacios: +5492215550000.
    /// </summary>
    public string? WhatsApp { get; private set; }

    /// <summary>
    /// Texto de la pestaña «Sobre mí» o «Sobre nosotros».
    /// </summary>
    public string? Sobre { get; private set; }

    /// <summary>
    /// Dónde queda el local. Solo en la página del estudio.
    /// </summary>
    public string? Ubicacion { get; private set; }

    /// <summary>
    /// Horarios de atención. Solo en la página del estudio.
    /// </summary>
    public string? Horarios { get; private set; }

    /// <summary>
    /// Si aparece en el sitio público.
    /// </summary>
    public bool Visible { get; private set; }

    public DateTimeOffset CreadaEn { get; }

    public DateTimeOffset ActualizadaEn { get; private set; }

    /// <summary>
    /// Crea la página del estudio, oculta, con la dirección armada a partir del nombre.
    /// </summary>
    public static PaginaPublica CrearDelEstudio(Guid estudioId, DatosPagina datos, DateTimeOffset ahora) =>
        Crear(estudioId, tatuadorId: null, datos, ahora);

    /// <summary>
    /// Crea la página de un artista, oculta, con la dirección armada a partir de su nombre.
    /// </summary>
    public static PaginaPublica CrearDeArtista(
        Guid estudioId,
        Guid tatuadorId,
        DatosPagina datos,
        DateTimeOffset ahora)
    {
        if (tatuadorId == Guid.Empty)
            throw new ArgumentException("El TatuadorId no puede estar vacío.", nameof(tatuadorId));

        return Crear(estudioId, tatuadorId, datos, ahora);
    }

    /// <summary>
    /// Cambia los datos del encabezado y del «Sobre». La dirección no cambia aunque cambie el nombre.
    /// </summary>
    public void Editar(DatosPagina datos, DateTimeOffset ahora)
    {
        FechaUtc.Validar(ahora, nameof(ahora));

        AplicarDatos(datos);
        ActualizadaEn = ahora;
    }

    /// <summary>
    /// Cambia la dirección a propósito. Los links que ya se compartieron con la anterior dejan de funcionar.
    /// </summary>
    public void CambiarDireccion(DireccionPublica direccion, DateTimeOffset ahora)
    {
        ArgumentNullException.ThrowIfNull(direccion);
        FechaUtc.Validar(ahora, nameof(ahora));

        Direccion = direccion;
        ActualizadaEn = ahora;
    }

    /// <summary>
    /// La muestra en el sitio público.
    /// </summary>
    public void Mostrar(DateTimeOffset ahora)
    {
        FechaUtc.Validar(ahora, nameof(ahora));

        Visible = true;
        ActualizadaEn = ahora;
    }

    /// <summary>
    /// La saca del sitio público sin borrar nada.
    /// </summary>
    public void Ocultar(DateTimeOffset ahora)
    {
        FechaUtc.Validar(ahora, nameof(ahora));

        Visible = false;
        ActualizadaEn = ahora;
    }

    private static PaginaPublica Crear(Guid estudioId, Guid? tatuadorId, DatosPagina datos, DateTimeOffset ahora)
    {
        if (estudioId == Guid.Empty)
            throw new ArgumentException("El EstudioId no puede estar vacío.", nameof(estudioId));

        ArgumentNullException.ThrowIfNull(datos);
        FechaUtc.Validar(ahora, nameof(ahora));

        var direccion = DireccionPublica.DesdeNombre(datos.Nombre);
        var pagina = new PaginaPublica(Guid.NewGuid(), estudioId, tatuadorId, direccion, ahora);
        pagina.AplicarDatos(datos);

        return pagina;
    }

    private void AplicarDatos(DatosPagina datos)
    {
        ArgumentNullException.ThrowIfNull(datos);

        var nombre = Texto.Obligatorio(datos.Nombre, MaxNombre, "El nombre", nameof(datos.Nombre));
        var bio = Texto.Opcional(datos.Bio, MaxBio, "La bio", nameof(datos.Bio));
        var instagram = NormalizarInstagram(datos.Instagram);
        var whatsApp = NormalizarWhatsApp(datos.WhatsApp);
        var sobre = Texto.Opcional(datos.Sobre, MaxSobre, "El texto de «Sobre»", nameof(datos.Sobre));
        var ubicacion = Texto.Opcional(datos.Ubicacion, MaxUbicacion, "La ubicación", nameof(datos.Ubicacion));
        var horarios = Texto.Opcional(datos.Horarios, MaxHorarios, "Los horarios", nameof(datos.Horarios));

        // La ubicación y los horarios son del local: la página de un artista no los tiene (doc 07, 7.4).
        if (!EsDelEstudio && (ubicacion is not null || horarios is not null))
            throw new ArgumentException(
                "La ubicación y los horarios van solo en la página del estudio.", nameof(datos));

        Nombre = nombre;
        Bio = bio;
        Instagram = instagram;
        WhatsApp = whatsApp;
        Sobre = sobre;
        Ubicacion = ubicacion;
        Horarios = horarios;
    }

    /// <summary>
    /// Acepta el usuario con o sin @ y lo guarda sin @: «@ink.mortall» queda «ink.mortall».
    /// </summary>
    private static string? NormalizarInstagram(string? instagram)
    {
        var usuario = Texto.Opcional(
            instagram?.Trim().TrimStart('@'), MaxInstagram, "El usuario de Instagram", nameof(DatosPagina.Instagram));

        if (usuario is not null && !usuario.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_'))
            throw new ArgumentException(
                "El usuario de Instagram solo puede tener letras, números, puntos y guiones bajos.",
                nameof(DatosPagina.Instagram));

        return usuario;
    }

    /// <summary>
    /// Acepta el número con espacios, guiones o paréntesis y lo guarda en formato internacional (E.164):
    /// «+54 9 221 555-0000» queda «+5492215550000». El link de WhatsApp necesita el código de país.
    /// </summary>
    private static string? NormalizarWhatsApp(string? whatsApp)
    {
        if (string.IsNullOrWhiteSpace(whatsApp))
            return null;

        var limpio = whatsApp.Trim();
        var digitos = new string(limpio.Where(char.IsAsciiDigit).ToArray());

        var esValido = limpio.StartsWith('+')
            && limpio.IndexOf('+', 1) < 0
            && limpio.All(c => char.IsAsciiDigit(c) || c is '+' or ' ' or '-' or '(' or ')')
            && digitos.Length is >= 8 and <= 15
            && digitos[0] != '0';

        if (!esValido)
            throw new ArgumentException(
                "El WhatsApp tiene que estar en formato internacional, con el código de país: " +
                "por ejemplo, +54 9 221 555-0000.",
                nameof(DatosPagina.WhatsApp));

        return "+" + digitos;
    }
}
