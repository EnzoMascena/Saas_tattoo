namespace Tato.Application.Estudios;

/// <summary>
/// Puerto que indica qué usuario está operando, qué roles tiene (D-03) y cuál es su perfil de tatuador. Sale de la
/// sesión autenticada, nunca de un dato que mande el navegador. Sin sesión, los Ids son null y los roles dan false.
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

    /// <summary>
    /// Perfil de tatuador del usuario, o null si no tatúa. Su página pública es la de este perfil (D-51).
    /// </summary>
    Guid? TatuadorId { get; }
}
