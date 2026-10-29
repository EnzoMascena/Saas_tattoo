using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tato.Domain.Portfolio;

namespace Tato.Infrastructure.Portfolio;

/// <summary>
/// Mapeo de <see cref="Foto"/> a la tabla Fotos. La relación con la publicación la define PublicacionConfiguration.
/// </summary>
internal sealed class FotoConfiguration : IEntityTypeConfiguration<Foto>
{
    public void Configure(EntityTypeBuilder<Foto> builder)
    {
        builder.ToTable("Fotos");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();

        builder.Property(f => f.EstudioId).IsRequired();
        builder.Property(f => f.PublicacionId).IsRequired();
        builder.Property(f => f.Orden).IsRequired();
        builder.Property(f => f.Ancho).IsRequired();
        builder.Property(f => f.Alto).IsRequired();
        builder.Property(f => f.SubidaEn).IsRequired();

        // No se pone índice único en (PublicacionId, Orden): ver la explicación de arriba.
        // EF ya crea un índice sobre PublicacionId porque es clave foránea.
    }
}
