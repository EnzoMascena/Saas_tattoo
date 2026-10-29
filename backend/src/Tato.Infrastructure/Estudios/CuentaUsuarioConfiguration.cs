using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tato.Domain.Estudios;

namespace Tato.Infrastructure.Estudios;

/// <summary>
/// Completa el mapeo que trae Identity para <see cref="CuentaUsuario"/> (tabla AspNetUsers).
/// </summary>
internal sealed class CuentaUsuarioConfiguration : IEntityTypeConfiguration<CuentaUsuario>
{
    public void Configure(EntityTypeBuilder<CuentaUsuario> builder)
    {
        // Cuenta → Usuario (1 a 1, mismo Id): no puede existir una cuenta sin su Usuario del dominio.
        builder.HasOne<Usuario>()
            .WithOne()
            .HasForeignKey<CuentaUsuario>(c => c.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
