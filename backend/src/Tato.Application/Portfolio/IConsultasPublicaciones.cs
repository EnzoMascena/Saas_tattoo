using Tato.Application.Comun;

namespace Tato.Application.Portfolio;

/// <summary>
/// Lecturas de publicaciones para el panel. Devuelven datos listos para mostrar, sin cargar las entidades del
/// dominio: leer no necesita las reglas, y así las consultas piden a la base solo las columnas que hacen falta.
/// Siempre son del estudio de la sesión (D-42).
/// </summary>
public interface IConsultasPublicaciones
{
    /// <summary>
    /// Lista las publicaciones del estudio, de la modificada más recientemente a la más vieja.
    /// </summary>
    Task<Pagina<PublicacionResumen>> ListarAsync(FiltroPublicaciones filtro, CancellationToken cancellationToken);

    /// <summary>
    /// Devuelve la publicación con sus fotos en orden, o null si no existe o es de otro estudio.
    /// </summary>
    Task<PublicacionDetalle?> ObtenerAsync(Guid id, CancellationToken cancellationToken);
}
