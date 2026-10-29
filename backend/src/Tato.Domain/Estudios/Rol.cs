namespace Tato.Domain.Estudios;

/// <summary>
/// Roles de usuario en el estudio (D-03).
/// </summary>
[Flags]
public enum Rol
{
    Duenio = 1,
    Tatuador = 2,
    Administrativo = 4
}
