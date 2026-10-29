using Tato.Domain.Estudios;

namespace Tato.Application.Estudios;

/// <summary>
/// Caso de uso: dar de alta un usuario con su cuenta y, si es Tatuador, con su perfil (P-19).
/// </summary>
public sealed class AltaUsuario
{
    private readonly IRegistroEstudios _registro;

    public AltaUsuario(IRegistroEstudios registro)
    {
        _registro = registro;
    }

    /// <summary>
    /// Crea el usuario. Los datos que el dominio rechaza lanzan ArgumentException; los problemas de la cuenta
    /// (contraseña corta, email repetido) vuelven en el resultado.
    /// La contraseña va aparte de la solicitud porque el ToString de un record muestra todas sus propiedades:
    /// así no termina en un log si alguien registra la solicitud (doc 06, 7.4).
    /// </summary>
    public async Task<ResultadoAlta> EjecutarAsync(
        SolicitudAltaUsuario solicitud,
        string contrasenia,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        var usuario = Usuario.Crear(solicitud.EstudioId, solicitud.Email, solicitud.Nombre, solicitud.Roles);

        // Todo Tatuador tiene su perfil: es lo que muestra el blog (doc 07).
        var perfilTatuador = usuario.Roles.HasFlag(Rol.Tatuador)
            ? Tatuador.Crear(usuario, solicitud.NombreArtistico ?? string.Empty, instagram: solicitud.Instagram)
            : null;

        if (!await _registro.ExisteEstudioAsync(solicitud.EstudioId, cancellationToken))
            return ResultadoAlta.Fallido([$"No existe un estudio con el Id {solicitud.EstudioId}."]);

        return await _registro.AgregarUsuarioAsync(usuario, perfilTatuador, contrasenia, cancellationToken);
    }
}

/// <summary>
/// Datos de un usuario nuevo. NombreArtistico e Instagram solo se usan si tiene el rol Tatuador.
/// </summary>
public sealed record SolicitudAltaUsuario(
    Guid EstudioId,
    string Email,
    string Nombre,
    Rol Roles,
    string? NombreArtistico = null,
    string? Instagram = null);
