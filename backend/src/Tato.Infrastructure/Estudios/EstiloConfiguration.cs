using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tato.Domain.Estudios;

namespace Tato.Infrastructure.Estudios;

/// <summary>
/// Mapeo de <see cref="Estilo"/> a la tabla Estilos (R-11).
/// </summary>
internal sealed class EstiloConfiguration : IEntityTypeConfiguration<Estilo>
{
    public void Configure(EntityTypeBuilder<Estilo> builder)
    {
        builder.ToTable("Estilos");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.EstudioId).IsRequired();

        builder.Property(e => e.Nombre)
            .HasMaxLength(ElementoCatalogo.MaxNombre)
            .IsRequired();

        builder.Property(e => e.Activo).IsRequired();

        // Un estudio no puede tener dos estilos con el mismo nombre. SQL Server compara sin
        // distinguir mayúsculas con su collation por defecto, así que «fineline» y «Fineline» chocan.
        builder.HasIndex(e => new { e.EstudioId, e.Nombre }).IsUnique();
    }
}
