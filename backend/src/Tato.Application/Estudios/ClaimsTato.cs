namespace Tato.Application.Estudios;

/// <summary>
/// Nombres de los claims propios que viajan en la cookie de sesión.
/// Los escribe Infrastructure al iniciar sesión y los lee la Api.
/// </summary>
public static class ClaimsTato
{
    /// <summary>
    /// Estudio del usuario logueado. Es la base del filtro por estudio (D-42).
    /// </summary>
    public const string EstudioId = "estudio_id";

    /// <summary>
    /// Nombre del usuario, para mostrarlo en el panel.
    /// </summary>
    public const string Nombre = "nombre";

    /// <summary>
    /// Perfil de tatuador del usuario, si tiene el rol Tatuador. Define cuál es su página pública (D-51).
    /// </summary>
    public const string TatuadorId = "tatuador_id";
}
