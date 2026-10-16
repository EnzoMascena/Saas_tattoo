using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tato.Application.Comun;
using Tato.Application.Portfolio;
using Tato.Domain.Portfolio;
using Tato.Infrastructure.Persistencia;

namespace Tato.Infrastructure.Portfolio;

/// <summary>
/// Implementación de <see cref="IRepositorioPaginas"/> con EF Core. El filtro por estudio (D-42) limita qué páginas
/// se obtienen. La dirección es la excepción: es única en toda la plataforma, así que se busca en todos los
/// estudios.
/// </summary>
internal sealed class RepositorioPaginas : IRepositorioPaginas
{
    /// <summary>
    /// Un rowversion de SQL Server ocupa siempre 8 bytes.
    /// </summary>
    private const int BytesVersion = 8;

    /// <summary>
    /// Errores de SQL Server por un valor repetido en un índice único (2601) o en una restricción única (2627).
    /// </summary>
    private const int IndiceUnicoRepetido = 2601;
    private const int RestriccionUnicaRepetida = 2627;

    private readonly TatoDbContext _db;

    public RepositorioPaginas(TatoDbContext db)
    {
        _db = db;
    }

    public Task<PaginaPublica?> ObtenerAsync(Guid id, CancellationToken cancellationToken) =>
        _db.PaginasPublicas.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> ExisteAsync(Guid? tatuadorId, CancellationToken cancellationToken) =>
        _db.PaginasPublicas.AnyAsync(p => p.TatuadorId == tatuadorId, cancellationToken);

    public Task<bool> DireccionEnUsoAsync(DireccionPublica direccion, CancellationToken cancellationToken) =>
        _db.PaginasPublicas
            .IgnoreQueryFilters([TatoDbContext.FiltroEstudio])
            .AnyAsync(p => p.Direccion == direccion, cancellationToken);

    public async Task AgregarAsync(PaginaPublica pagina, CancellationToken cancellationToken)
    {
        _db.PaginasPublicas.Add(pagina);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException excepcion) when (EsValorRepetido(excepcion))
        {
            // El caso de uso ya controló las dos cosas, pero otra persona pudo ganarle de mano justo antes de
            // guardar. Los índices únicos de la base son la garantía final (RN-BLG-07).
            throw new ConflictoDeVersionException(
                "Otra persona acaba de crear esta página o de tomar su dirección. Recargá y volvé a intentar.");
        }
    }

    public async Task<string> GuardarCambiosAsync(
        PaginaPublica pagina,
        string versionEsperada,
        CancellationToken cancellationToken)
    {
        var entrada = _db.Entry(pagina);

        // Las publicaciones y las páginas usan la misma columna de versión.
        var version = entrada.Property<byte[]>(PublicacionConfiguration.ColumnaVersion);
        version.OriginalValue = DesdeBase64(versionEsperada);

        // Como en las publicaciones: la fila se actualiza siempre, así la versión se controla aunque no cambie
        // ningún dato (por ejemplo, al mostrar una página que ya estaba visible).
        entrada.Property(p => p.ActualizadaEn).IsModified = true;

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoDeVersionException(
                "La página cambió desde que la abriste. Recargala para ver los cambios y volvé a intentar.");
        }
        catch (DbUpdateException excepcion) when (EsValorRepetido(excepcion))
        {
            throw new ConflictoDeVersionException(
                "Otra página acaba de tomar esa dirección. Recargá y elegí otra.");
        }

        return Convert.ToBase64String(version.CurrentValue);
    }

    private static bool EsValorRepetido(DbUpdateException excepcion) =>
        excepcion.InnerException is SqlException { Number: IndiceUnicoRepetido or RestriccionUnicaRepetida };

    private static byte[] DesdeBase64(string version)
    {
        var bytes = new byte[BytesVersion];

        if (string.IsNullOrWhiteSpace(version)
            || !Convert.TryFromBase64String(version, bytes, out var escritos)
            || escritos != BytesVersion)
            throw new ArgumentException("La versión de la página no es válida.", nameof(version));

        return bytes;
    }
}
