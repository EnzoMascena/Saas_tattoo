using Tato.Domain.Comun;

namespace Tato.Domain.Estudios;

/// <summary>
/// Un usuario del estudio, con email, nombre y roles (D-03).
/// </summary>
public sealed class Usuario : IPerteneceAEstudio
{
    public const int MaxEmail = 256;
    public const int MaxNombre = 100;

    /// <summary>
    /// Todos los roles que existen. Sirve para rechazar valores que no corresponden a ninguno.
    /// </summary>
    private const Rol RolesValidos = Rol.Duenio | Rol.Tatuador | Rol.Administrativo;

    private Usuario(
        Guid id,
        Guid estudioId,
        string email,
        string nombre,
        Rol roles)
    {
        Id = id;
        EstudioId = estudioId;
        Email = email;
        Nombre = nombre;
        Roles = roles;
    }

    /// <summary>
    /// Identificador único del usuario (UUID). Lo genera el dominio al crearlo.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Estudio al que pertenece el usuario (D-42).
    /// </summary>
    public Guid EstudioId { get; }

    /// <summary>
    /// Email del usuario. Lo usa para iniciar sesión.
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    public string Nombre { get; private set; }

    /// <summary>
    /// Roles del usuario en el estudio (D-03).
    /// </summary>
    public Rol Roles { get; private set; }

    public static Usuario Crear(
        Guid estudioId,
        string email,
        string nombre,
        Rol roles)
    {
        if (estudioId == Guid.Empty)
            throw new ArgumentException("El EstudioId no puede estar vacío.", nameof(estudioId));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email no puede estar vacío.", nameof(email));

        var emailLimpio = email.Trim();

        if (emailLimpio.Length > MaxEmail)
            throw new ArgumentException($"El email no puede exceder {MaxEmail} caracteres.", nameof(email));

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));

        var nombreLimpio = nombre.Trim();

        if (nombreLimpio.Length > MaxNombre)
            throw new ArgumentException($"El nombre no puede exceder {MaxNombre} caracteres.", nameof(nombre));

        ValidarRoles(roles);

        return new Usuario(Guid.NewGuid(), estudioId, emailLimpio, nombreLimpio, roles);
    }

    /// <summary>
    /// Cambia los roles del usuario. Solo el Dueño puede hacer esto.
    /// </summary>
    public void CambiarRoles(Rol nuevosRoles)
    {
        ValidarRoles(nuevosRoles);
        Roles = nuevosRoles;
    }

    /// <summary>
    /// Valida que los roles cumplan con las reglas de negocio (D-03).
    /// </summary>
    private static void ValidarRoles(Rol roles)
    {
        if (roles == 0)
            throw new ArgumentException("El usuario debe tener al menos un rol.", nameof(roles));

        // Si quedan bits prendidos fuera de los roles válidos, es un valor como (Rol)8.
        if ((roles & ~RolesValidos) != 0)
            throw new ArgumentException("El usuario tiene un rol desconocido.", nameof(roles));

        bool esDuenio = roles.HasFlag(Rol.Duenio);
        bool esTatuador = roles.HasFlag(Rol.Tatuador);
        bool esAdministrativo = roles.HasFlag(Rol.Administrativo);

        // Si no es Dueño, no puede ser Tatuador y Administrativo juntos.
        if (!esDuenio && esTatuador && esAdministrativo)
            throw new ArgumentException(
                "Un usuario que no es Dueño no puede ser Tatuador y Administrativo a la vez.", nameof(roles));
    }
}