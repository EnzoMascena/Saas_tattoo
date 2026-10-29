namespace Tato.Domain.Comun;

/// <summary>
/// Entidad que pertenece a un estudio (D-42). Todas las que la implementan quedan aisladas:
/// solo se leen y se guardan con la sesión de su propio estudio.
/// </summary>
public interface IPerteneceAEstudio
{
    Guid EstudioId { get; }
}
