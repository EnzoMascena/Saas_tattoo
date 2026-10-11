using Tato.Domain.Comun;

namespace Tato.Domain.Estudios;

/// <summary>
/// Perfil profesional de un usuario con rol Tatuador. Guarda los datos que el usuario
/// no tiene y que se muestran en su perfil público del blog (doc 07).
/// </summary>
public sealed class Tatuador : IPerteneceAEstudio
{
    public const int MaxNombreArtistico = 100;
    public const int MaxBio = 1000;
    public const int MaxInstagram = 30;

    private Tatuador(
        Guid id,
        Guid usuarioId,
        Guid estudioId,
        string nombreArtistico,
        string? bio,
        string? instagram)
    {
        Id = id;
        UsuarioId = usuarioId;
        EstudioId = estudioId;
        NombreArtistico = nombreArtistico;
        Bio = bio;
        Instagram = instagram;
    }

    /// <summary>
    /// Identificador único del perfil (UUID). Lo genera el dominio al crearlo.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Usuario al que pertenece el perfil.
    /// </summary>
    public Guid UsuarioId { get; }

    /// <summary>
    /// Estudio al que pertenece el perfil (D-42). Se copia del usuario.
    /// </summary>
    public Guid EstudioId { get; }

    /// <summary>
    /// Nombre con el que el tatuador firma sus trabajos.
    /// </summary>
    public string NombreArtistico { get; private set; }

    /// <summary>
    /// Presentación del tatuador para su perfil público. Opcional.
    /// </summary>
    public string? Bio { get; private set; }

    /// <summary>
    /// Usuario de Instagram, sin la arroba. Opcional.
    /// </summary>
    public string? Instagram { get; private set; }

    /// <summary>
    /// Crea el perfil de tatuador de un usuario que tiene el rol Tatuador.
    /// </summary>
    public static Tatuador Crear(
        Usuario usuario,
        string nombreArtistico,
        string? bio = null,
        string? instagram = null)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        if (!usuario.Roles.HasFlag(Rol.Tatuador))
            throw new ArgumentException("El usuario debe tener el rol Tatuador.", nameof(usuario));

        var (nombreValidado, bioValidada, instagramValidado) = Validar(nombreArtistico, bio, instagram);

        return new Tatuador(
            Guid.NewGuid(),
            usuario.Id,
            usuario.EstudioId,
            nombreValidado,
            bioValidada,
            instagramValidado);
    }

    /// <summary>
    /// Actualiza los datos del perfil público. Si la bio o el Instagram vienen vacíos, se borran.
    /// </summary>
    public void ActualizarPerfil(string nombreArtistico, string? bio, string? instagram)
    {
        (NombreArtistico, Bio, Instagram) = Validar(nombreArtistico, bio, instagram);
    }

    /// <summary>
    /// Valida los datos del perfil y los devuelve normalizados.
    /// </summary>
    private static (string NombreArtistico, string? Bio, string? Instagram) Validar(
        string nombreArtistico,
        string? bio,
        string? instagram)
    {
        if (string.IsNullOrWhiteSpace(nombreArtistico))
            throw new ArgumentException("El nombre artístico no puede estar vacío.", nameof(nombreArtistico));

        var nombreValidado = nombreArtistico.Trim();

        if (nombreValidado.Length > MaxNombreArtistico)
            throw new ArgumentException(
                $"El nombre artístico no puede exceder {MaxNombreArtistico} caracteres.", nameof(nombreArtistico));

        var bioValidada = VacioANull(bio);

        if (bioValidada is not null && bioValidada.Length > MaxBio)
            throw new ArgumentException($"La bio no puede exceder {MaxBio} caracteres.", nameof(bio));

        // Se saca la arroba por si la escriben: se guarda solo el usuario ("enzo_tattoos").
        var instagramValidado = VacioANull(instagram?.Trim().TrimStart('@'));

        if (instagramValidado is not null && instagramValidado.Length > MaxInstagram)
            throw new ArgumentException(
                $"El usuario de Instagram no puede exceder {MaxInstagram} caracteres.", nameof(instagram));

        return (nombreValidado, bioValidada, instagramValidado);
    }

    /// <summary>
    /// Convierte los textos vacíos o con solo espacios en null, para que «no tiene» se guarde de una sola forma.
    /// </summary>
    private static string? VacioANull(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
