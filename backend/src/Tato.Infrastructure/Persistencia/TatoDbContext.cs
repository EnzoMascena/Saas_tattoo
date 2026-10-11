using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tato.Application.Estudios;
using Tato.Domain.Estudios;
using Tato.Domain.Portfolio;
using Tato.Infrastructure.Estudios;

namespace Tato.Infrastructure.Persistencia;

/// <summary>
/// Contexto de EF Core del sistema: una sola base compartida por todos los estudios (D-42).
/// Hereda de IdentityUserContext para guardar las cuentas de login (R-09). No usa las tablas
/// de roles de Identity: los roles salen de Usuario.Roles.
/// El mapeo de cada entidad vive en su propia IEntityTypeConfiguration, en la carpeta de su módulo.
/// </summary>
public sealed class TatoDbContext : IdentityUserContext<CuentaUsuario, Guid>
{
    /// <summary>
    /// Nombre del filtro por estudio. Sirve para saltearlo a propósito, y solo a ese:
    /// <c>IgnoreQueryFilters([TatoDbContext.FiltroEstudio])</c>.
    /// </summary>
    public const string FiltroEstudio = "Estudio";

    private readonly IEstudioActual _estudioActual;

    public TatoDbContext(DbContextOptions<TatoDbContext> options, IEstudioActual estudioActual)
        : base(options)
    {
        _estudioActual = estudioActual;
    }

    // Solo lectura (=> Set<T>()): no generan el warning de nullable y nadie puede reemplazarlos.
    public DbSet<Estudio> Estudios => Set<Estudio>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Tatuador> Tatuadores => Set<Tatuador>();

    public DbSet<Estilo> Estilos => Set<Estilo>();

    public DbSet<ZonaCuerpo> ZonasCuerpo => Set<ZonaCuerpo>();

    /// <summary>
    /// Publicaciones del blog. Sus fotos se cargan con Include(p => p.Fotos): no tienen DbSet propio porque
    /// solo se tocan a través de su publicación.
    /// </summary>
    public DbSet<Publicacion> Publicaciones => Set<Publicacion>();

    /// <summary>
    /// Estudio de la sesión. El filtro lo lee en cada consulta, del contexto que la ejecuta.
    /// </summary>
    private Guid? EstudioIdActual => _estudioActual.EstudioId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primero el modelo de Identity (AspNetUsers y sus tablas); después el nuestro.
        base.OnModelCreating(modelBuilder);

        // Aplica todas las IEntityTypeConfiguration<T> de este proyecto.
        // Una entidad nueva solo necesita su clase de configuración: no hay que registrarla acá.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TatoDbContext).Assembly);

        AplicarFiltroEstudio(modelBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidarEstudio();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ValidarEstudio();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Aplica el filtro por estudio a todas las entidades que implementan <see cref="IPerteneceAEstudio"/>:
    /// olvidarse un WHERE no expone datos de otro estudio (doc 06, 4). Sin estudio en la sesión no devuelve nada.
    /// </summary>
    private void AplicarFiltroEstudio(ModelBuilder modelBuilder)
    {
        var aplicarA = typeof(TatoDbContext).GetMethod(
            nameof(AplicarFiltroEstudioA), BindingFlags.NonPublic | BindingFlags.Instance)!;

        // ToList: se termina de leer el modelo antes de modificarlo.
        var tipos = modelBuilder.Model.GetEntityTypes()
            .Select(t => t.ClrType)
            .Where(t => typeof(IPerteneceAEstudio).IsAssignableFrom(t))
            .ToList();

        foreach (var tipo in tipos)
            aplicarA.MakeGenericMethod(tipo).Invoke(this, [modelBuilder]);
    }

    private void AplicarFiltroEstudioA<TEntidad>(ModelBuilder modelBuilder)
        where TEntidad : class, IPerteneceAEstudio =>
        modelBuilder.Entity<TEntidad>().HasQueryFilter(FiltroEstudio, e => e.EstudioId == EstudioIdActual);

    /// <summary>
    /// El filtro protege las lecturas; esto protege las escrituras. Con un estudio en la sesión, todo lo que
    /// se guarda tiene que ser de ese estudio. Sin sesión (comandos de consola) no hay estudio con qué comparar.
    /// </summary>
    private void ValidarEstudio()
    {
        if (EstudioIdActual is not { } estudioId)
            return;

        var hayDatosDeOtroEstudio = ChangeTracker.Entries<IPerteneceAEstudio>()
            .Any(entrada =>
                (entrada.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                && entrada.Entity.EstudioId != estudioId);

        if (hayDatosDeOtroEstudio)
            throw new InvalidOperationException("No se pueden guardar datos de otro estudio (D-42).");
    }
}