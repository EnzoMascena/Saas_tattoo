using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tato.Domain.Estudios;

namespace Tato.Infrastructure.Estudios;

/// <summary>
/// Mapeo de <see cref="ZonaCuerpo"/> a la tabla ZonasCuerpo (R-11).
/// </summary>
internal sealed class ZonaCuerpoConfiguration : IEntityTypeConfiguration<ZonaCuerpo>
{
    public void Configure(EntityTypeBuilder<ZonaCuerpo> builder)
    {
        builder.ToTable("ZonasCuerpo");

        builder.HasKey(z => z.Id);
        builder.Property(z => z.Id).ValueGeneratedNever();

        builder.Property(z => z.EstudioId).IsRequired();

        builder.Property(z => z.Nombre)
            .HasMaxLength(ElementoCatalogo.MaxNombre)
            .IsRequired();

        builder.Property(z => z.Activo).IsRequired();

        // Mismo criterio que Estilo: no puede haber dos zonas con el mismo nombre en un estudio.
        builder.HasIndex(z => new { z.EstudioId, z.Nombre }).IsUnique();
    }
}
