using Tato.Application.Comun;
using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Lecturas del sitio público, sin sesión. Como no hay estudio en la sesión, la implementación no puede apoyarse en
/// el filtro por estudio (D-42): filtra a mano por página. Solo devuelve páginas visibles y, de cada una, sus
/// publicaciones Publicadas (RN-BLG-01, RN-BLG-12). Lo que no se ve da null, igual que lo que no existe.
/// </summary>
public interface IConsultasDelSitio
{
    Task<PaginaDelSitio?> ObtenerPaginaAsync(DireccionPublica direccion, CancellationToken cancellationToken);

    /// <summary>
    /// Las publicaciones de la página, en el orden de RN-BLG-13: primero las fijadas y después de la publicada más
    /// recientemente a la más vieja. Null si la página no se ve.
    /// </summary>
    Task<Pagina<PublicacionDelSitio>?> ListarPublicacionesAsync(
        DireccionPublica direccion,
        FiltroPublicacionesDelSitio filtro,
        CancellationToken cancellationToken);

    /// <summary>
    /// Una publicación de esa página. Null si no es de esa página o no se ve.
    /// </summary>
    Task<PublicacionDelSitioDetalle?> ObtenerPublicacionAsync(
        DireccionPublica direccion,
        Guid id,
        CancellationToken cancellationToken);

    /// <summary>
    /// Dónde está una foto que se puede mostrar: de una publicación Publicada de una página visible, o la foto o logo
    /// o la de «Sobre» de una página visible. Null en cualquier otro caso.
    /// </summary>
    Task<UbicacionFoto?> UbicarFotoAsync(Guid fotoId, CancellationToken cancellationToken);
}
