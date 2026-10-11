using Microsoft.EntityFrameworkCore;
using Tato.Application.Portfolio;
using Tato.Domain.Portfolio;
using Tato.Infrastructure.Persistencia;

namespace Tato.Infrastructure.Portfolio;

/// <summary>
/// Implementación de <see cref="IRepositorioPublicaciones"/> con EF Core. Carga la publicación con sus fotos
/// y guarda los cambios. El filtro por estudio (D-42) ya limita qué publicaciones se pueden obtener.
/// </summary>
internal sealed class RepositorioPublicaciones : IRepositorioPublicaciones
{
    private readonly TatoDbContext _db;

    public RepositorioPublicaciones(TatoDbContext db)
    {
        _db = db;
    }

    public Task<Publicacion?> ObtenerAsync(Guid id, CancellationToken cancellationToken) =>
        _db.Publicaciones
            .Include(p => p.Fotos)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task AgregarAsync(Publicacion publicacion, CancellationToken cancellationToken)
    {
        _db.Publicaciones.Add(publicacion);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}