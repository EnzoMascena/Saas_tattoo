using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tato.Domain.Estudios;

namespace Tato.Infrastructure.Estudios;

/// <summary>
/// Mapeo de <see cref="Tatuador"/> (perfil profesional de un usuario) a la tabla Tatuadores.
/// </summary>
internal sealed class TatuadorConfiguration : IEntityTypeConfiguration<Tatuador>
{
    public void Configure(EntityTypeBuilder<Tatuador> builder)
    {
        builder.ToTable("Tatuadores");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        // Sin setter: se mapean explícito.
        builder.Property(t => t.UsuarioId);
        builder.Property(t => t.EstudioId);

        builder.Property(t => t.NombreArtistico)
            .HasMaxLength(Tatuador.MaxNombreArtistico)
            .IsRequired();

        // Opcionales: string? en el dominio, columnas NULL en la base.
        builder.Property(t => t.Bio).HasMaxLength(Tatuador.MaxBio);
        builder.Property(t => t.Instagram).HasMaxLength(Tatuador.MaxInstagram);

        // Tatuador → Usuario (1 a 0..1): un usuario tiene como máximo un perfil Tatuador.
        // Por ser uno a uno, EF crea un índice único sobre UsuarioId.
        builder.HasOne<Usuario>()
            .WithOne()
            .HasForeignKey<Tatuador>(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tatuador → Estudio (N a 1): toda tabla del estudio lleva EstudioId con su FK (D-42).
        builder.HasOne<Estudio>()
            .WithMany()
            .HasForeignKey(t => t.EstudioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
