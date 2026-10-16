namespace Tato.Application.Portfolio;

/// <summary>
/// Lecturas de páginas públicas para el panel, sin cargar las entidades del dominio. Siempre son del estudio de la
/// sesión (D-42).
/// </summary>
public interface IConsultasPaginas
{
    /// <summary>
    /// Las páginas del estudio: la del estudio y las de sus artistas, las que ya se crearon.
    /// </summary>
    Task<IReadOnlyList<PaginaDetalle>> ListarAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Una página, o null si no existe o es de otro estudio.
    /// </summary>
    Task<PaginaDetalle?> ObtenerAsync(Guid id, CancellationToken cancellationToken);
}
