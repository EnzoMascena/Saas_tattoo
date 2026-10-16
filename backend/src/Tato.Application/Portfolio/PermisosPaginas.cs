using Tato.Application.Comun;
using Tato.Application.Estudios;

namespace Tato.Application.Portfolio;

/// <summary>
/// Quién gestiona cada página (doc 07, sección 5): la de un artista, solo ese artista; la del estudio, los Dueños.
/// Gestionar es crearla, cambiar sus datos y su dirección, y mostrarla u ocultarla.
/// </summary>
internal static class PermisosPaginas
{
    /// <param name="tatuadorId">El artista de la página, o null si es la del estudio.</param>
    public static bool PuedeGestionar(IUsuarioActual usuario, Guid? tatuadorId) =>
        tatuadorId is { } id ? id == usuario.TatuadorId : usuario.EsDuenio;

    public static void ExigirGestionar(IUsuarioActual usuario, Guid? tatuadorId)
    {
        if (PuedeGestionar(usuario, tatuadorId))
            return;

        throw new AccesoDenegadoException(tatuadorId is null
            ? "Solo un Dueño puede gestionar la página del estudio."
            : "Solo el artista de esta página puede gestionarla.");
    }
}
