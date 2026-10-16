namespace Tato.Application.Estudios;

/// <summary>
/// Puerto que indica qué usuario está operando y qué roles tiene (D-03). Sale de la sesión autenticada, nunca de
/// un dato que mande el navegador. Sin sesión, el Id es null y los roles dan false.
/// <para>
/// Expone los roles como preguntas y no como el enum Rol del dominio de Estudios: así otros módulos deciden sus
/// permisos sin usar clases de Estudios (D-38).
/// </para>
/// </summary>
public interface IUsuarioActual
{
    Guid? UsuarioId { get; }

    bool EsDuenio { get; }

    bool EsAdministrativo { get; }
}
