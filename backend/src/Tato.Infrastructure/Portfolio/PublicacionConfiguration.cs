using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tato.Domain.Portfolio;

namespace Tato.Infrastructure.Portfolio;

/// <summary>
/// Mapeo de <see cref="Publicacion"/> a la tabla Publicaciones. Sus fotos se guardan en la tabla Fotos.
/// </summary>
internal sealed class PublicacionConfiguration : IEntityTypeConfiguration<Publicacion>
{
    /// <summary>
    /// Columna de concurrencia optimista. Es una propiedad shadow: existe en la base, no en la clase del dominio.
    /// </summary>
    internal const string ColumnaVersion = "RowVersion";

    public void Configure(EntityTypeBuilder<Publicacion> builder)
    {
        builder.ToTable("Publicaciones");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.EstudioId).IsRequired();
        builder.Property(p => p.CreadaPor).IsRequired();
        builder.Property(p => p.CreadaEn).IsRequired();
        builder.Property(p => p.ActualizadaEn).IsRequired();
        builder.Property(p => p.PublicadaEn);

        builder.Property(p => p.Titulo)
            .HasMaxLength(Publicacion.MaxTitulo)
            .IsRequired();

        builder.Property(p => p.Descripcion)
            .HasMaxLength(Publicacion.MaxDescripcion);

        builder.Property(p => p.FechaTrabajo);
        builder.Property(p => p.EstiloId);
        builder.Property(p => p.ZonaCuerpoId);
        builder.Property(p => p.TatuadorId);

        builder.Property(p => p.Destacada).IsRequired();
        builder.Property(p => p.Estado).IsRequired();
        builder.Property(p => p.Tipo).IsRequired();

        builder.Property(p => p.ConsentimientoConfirmadoPor);
        builder.Property(p => p.ConsentimientoConfirmadoEn);

        // Concurrencia optimista (doc 04): si dos personas editan la misma publicación,
        // la segunda recibe un error y no pisa los cambios de la primera.
        // Es una columna solo de la base, así que el dominio no la conoce.
        builder.Property<byte[]>(ColumnaVersion).IsRowVersion();

        // La navegación es la propiedad Fotos. EF encuentra solo el campo _fotos por su nombre,
        // y con PropertyAccessMode.Field lee y escribe ahí, no en la copia ordenada que devuelve Fotos.
        builder.HasMany(p => p.Fotos)
            .WithOne()
            .HasForeignKey(f => f.PublicacionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Fotos).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Para el panel («Mis publicaciones» filtrada por estado) y para el sitio público.
        builder.HasIndex(p => new { p.EstudioId, p.Estado });
    }
}
