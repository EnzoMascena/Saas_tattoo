using Microsoft.EntityFrameworkCore;
using Tato.Application.Comun;
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
    /// <summary>
    /// Un rowversion de SQL Server ocupa siempre 8 bytes.
    /// </summary>
    private const int BytesVersion = 8;

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

    public async Task<string> GuardarCambiosAsync(
        Publicacion publicacion,
        string versionEsperada,
        CancellationToken cancellationToken)
    {
        var version = _db.Entry(publicacion).Property<byte[]>(PublicacionConfiguration.ColumnaVersion);

        // EF compara contra el valor original. Por defecto es el que acaba de leer; lo reemplazamos por el que
        // tenía el panel, para detectar también los cambios que pasaron mientras la persona editaba.
        version.OriginalValue = DesdeBase64(versionEsperada);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoDeVersionException(
                "La publicación cambió desde que la abriste. Recargala para ver los cambios y volvé a intentar.");
        }

        // Después de guardar, EF ya leyó de la base la versión nueva.
        return Convert.ToBase64String(version.CurrentValue);
    }

    private static byte[] DesdeBase64(string version)
    {
        var bytes = new byte[BytesVersion];

        if (string.IsNullOrWhiteSpace(version)
            || !Convert.TryFromBase64String(version, bytes, out var escritos)
            || escritos != BytesVersion)
            throw new ArgumentException("La versión de la publicación no es válida.", nameof(version));

        return bytes;
    }
}
