using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tato.Domain.Estudios;

namespace Tato.Infrastructure.Estudios;

/// <summary>
/// Mapeo de <see cref="Usuario"/> a la tabla Usuarios.
/// </summary>
internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        // Sin setter, igual que Id: se mapea explícito.
        builder.Property(u => u.EstudioId);

        builder.Property(u => u.Email)
            .HasMaxLength(Usuario.MaxEmail)
            .IsRequired();

        builder.Property(u => u.Nombre)
            .HasMaxLength(Usuario.MaxNombre)
            .IsRequired();

        // Rol es [Flags]: se guarda como int (Dueño + Tatuador = 3).
        // Las combinaciones válidas las controla el dominio (doc 04).
        builder.Property(u => u.Roles).IsRequired();

        // Único en toda la plataforma, no por estudio: el login es email + contraseña, sin elegir estudio.
        builder.HasIndex(u => u.Email).IsUnique();

        // Usuario → Estudio (N a 1), sin cascada: los registros no se borran (RN-VIS-06).
        builder.HasOne<Estudio>()
            .WithMany()
            .HasForeignKey(u => u.EstudioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
