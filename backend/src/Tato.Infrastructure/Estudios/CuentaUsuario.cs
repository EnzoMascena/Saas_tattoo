using Microsoft.AspNetCore.Identity;
using Tato.Domain.Estudios;

namespace Tato.Infrastructure.Estudios;

/// <summary>
/// Cuenta para iniciar sesión de un <see cref="Usuario"/> (R-09). Es de Identity: guarda el hash de la
/// contraseña, el bloqueo por intentos fallidos y el security stamp. El dominio no la conoce (D-37).
/// Comparte el Id con su Usuario.
/// </summary>
public sealed class CuentaUsuario : IdentityUser<Guid>
{
    /// <summary>
    /// Crea la cuenta de un usuario. El email es también el nombre de usuario para iniciar sesión.
    /// La contraseña se asigna aparte, con UserManager.CreateAsync(cuenta, contraseña).
    /// </summary>
    public static CuentaUsuario Crear(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        return new CuentaUsuario
        {
            Id = usuario.Id,
            UserName = usuario.Email,
            Email = usuario.Email,
        };
    }
}
