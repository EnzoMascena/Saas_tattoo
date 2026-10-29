namespace Tato.Application.Estudios;

/// <summary>
/// Puerto que indica el estudio de quien está operando (D-42). Sale de la sesión autenticada, nunca de un dato
/// que mande el navegador. Es null cuando no hay sesión: comandos de consola, inicio de sesión o sitio público.
/// </summary>
public interface IEstudioActual
{
    Guid? EstudioId { get; }
}
