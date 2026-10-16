using Tato.Application.Comun;
using Tato.Application.Estudios;
using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Quién puede hacer qué con las publicaciones (P-21, confirmada con el estudio piloto):
/// <list type="bullet">
/// <item>Dueño y Administrativo: crean, editan, publican y ocultan todo.</item>
/// <item>Tatuador: crea y edita las publicaciones que creó él; no publica ni oculta.</item>
/// </list>
/// Crear no está acá porque lo puede hacer cualquiera con sesión.
/// </summary>
internal static class PermisosPublicaciones
{
    public static void ExigirEditar(IUsuarioActual usuario, Publicacion publicacion)
    {
        if (GestionaTodo(usuario) || publicacion.CreadaPor == usuario.UsuarioId)
            return;

        throw new AccesoDenegadoException("Solo podés editar las publicaciones que creaste vos.");
    }

    public static void ExigirPublicarUOcultar(IUsuarioActual usuario)
    {
        if (GestionaTodo(usuario))
            return;

        throw new AccesoDenegadoException("Solo un Dueño o un Administrativo puede publicar u ocultar publicaciones.");
    }

    private static bool GestionaTodo(IUsuarioActual usuario) => usuario.EsDuenio || usuario.EsAdministrativo;
}
