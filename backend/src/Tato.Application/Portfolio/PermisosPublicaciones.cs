using Tato.Application.Comun;
using Tato.Application.Estudios;
using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Quién puede hacer qué con las publicaciones, según la página en la que están (D-51, doc 07, sección 5):
/// <list type="bullet">
/// <item>Página de un artista (la publicación tiene TatuadorId): solo ese artista crea, edita, publica y oculta.
/// Nadie más, tampoco un Dueño.</item>
/// <item>Página del estudio (sin TatuadorId): cualquiera con sesión crea; Dueño y Administrativo editan, publican
/// y ocultan todo; un Tatuador edita lo que creó, sin publicar ni ocultar.</item>
/// </list>
/// </summary>
internal static class PermisosPublicaciones
{
    /// <summary>
    /// Para crear una publicación en una página, o moverla a otra: la del estudio la puede usar cualquiera con
    /// sesión; la de un artista, solo él.
    /// </summary>
    public static void ExigirPublicarEnPagina(IUsuarioActual usuario, Guid? tatuadorId)
    {
        if (tatuadorId is null || tatuadorId == usuario.TatuadorId)
            return;

        throw new AccesoDenegadoException("Solo podés publicar en tu propia página o en la del estudio.");
    }

    /// <summary>
    /// Para editar los datos, las fotos o el consentimiento de una publicación.
    /// </summary>
    public static void ExigirEditar(IUsuarioActual usuario, Publicacion publicacion)
    {
        if (publicacion.TatuadorId is { } tatuadorId)
        {
            if (tatuadorId == usuario.TatuadorId)
                return;

            throw new AccesoDenegadoException("Solo el artista de esta página puede editar sus publicaciones.");
        }

        if (GestionaElEstudio(usuario) || publicacion.CreadaPor == usuario.UsuarioId)
            return;

        throw new AccesoDenegadoException("Solo podés editar las publicaciones del estudio que creaste vos.");
    }

    /// <summary>
    /// Para publicar u ocultar una publicación.
    /// </summary>
    public static void ExigirPublicarUOcultar(IUsuarioActual usuario, Publicacion publicacion)
    {
        if (publicacion.TatuadorId is { } tatuadorId)
        {
            if (tatuadorId == usuario.TatuadorId)
                return;

            throw new AccesoDenegadoException(
                "Solo el artista de esta página puede publicar u ocultar sus publicaciones.");
        }

        if (GestionaElEstudio(usuario))
            return;

        throw new AccesoDenegadoException(
            "Solo un Dueño o un Administrativo puede publicar u ocultar las publicaciones del estudio.");
    }

    private static bool GestionaElEstudio(IUsuarioActual usuario) => usuario.EsDuenio || usuario.EsAdministrativo;
}
