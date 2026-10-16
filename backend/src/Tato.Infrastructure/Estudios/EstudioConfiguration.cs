using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tato.Domain.Estudios;

namespace Tato.Infrastructure.Estudios;

/// <summary>
/// Mapeo de <see cref="Estudio"/> (tenant raíz) a la tabla Estudios.
/// </summary>
internal sealed class EstudioConfiguration : IEntityTypeConfiguration<Estudio>
{
    public void Configure(EntityTypeBuilder<Estudio> builder)
    {
        builder.ToTable("Estudios");

        // EF mapea por su cuenta solo las propiedades con setter; Id no tiene, así que se configura acá.
        // Lo genera el dominio (Guid.NewGuid()), nunca la base.
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Nombre)
            .HasMaxLength(Estudio.MaxNombre)
            .IsRequired();

        builder.Property(e => e.Plan)
            .HasMaxLength(Estudio.MaxPlan)
            .IsRequired();

        builder.Property(e => e.ZonaHoraria)
            .HasMaxLength(Estudio.MaxZonaHoraria)
            .IsRequired();

        // Parámetros de negocio (D-34). Los valores por defecto los pone el dominio, no la base.
        builder.Property(e => e.DiasVencimientoSenia).IsRequired();
        builder.Property(e => e.MaxReagendamientos).IsRequired();
        builder.Property(e => e.MaxFotosPorPublicacion).IsRequired();
        builder.Property(e => e.MaxFotosPorPublicacion).IsRequired();
    }
}