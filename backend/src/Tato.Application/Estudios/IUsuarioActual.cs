namespace Tato.Application.Estudios;

/// <summary>
/// Puerto que indica qué usuario está operando. Sale de la sesión autenticada, nunca de un dato que mande el
/// navegador. Es null cuando no hay sesión.
/// </summary>
public interface IUsuarioActual
{
    Guid? UsuarioId { get; }
}
