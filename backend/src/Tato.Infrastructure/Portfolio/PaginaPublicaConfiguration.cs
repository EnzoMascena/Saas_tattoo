using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tato.Domain.Portfolio;

namespace Tato.Infrastructure.Portfolio;

/// <summary>
/// Mapeo de <see cref="PaginaPublica"/> a la tabla PaginasPublicas. Los índices únicos repiten en la base las
/// reglas que controla el caso de uso, por si dos pedidos llegan al mismo tiempo (RN-BLG-07).
/// </summary>
internal sealed class PaginaPublicaConfiguration : IEntityTypeConfiguration<PaginaPublica>
{
    /// <summary>
    /// Columna de concurrencia optimista: dos Dueños pueden editar la página del estudio a la vez.
    /// </summary>
    internal const string ColumnaVersion = "RowVersion";

    public void Configure(EntityTypeBuilder<PaginaPublica> builder)
    {
        builder.ToTable("PaginasPublicas");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.EstudioId).IsRequired();
        builder.Property(p => p.TatuadorId);

        // La dirección se guarda como texto y, al leerla, se vuelve a validar. Si algún día se reserva una palabra
        // que ya usa una página, hay que cambiarle la dirección a esa página antes de desplegar.
        builder.Property(p => p.Direccion)
            .HasConversion(direccion => direccion.Valor, valor => DireccionPublica.Crear(valor))
            .HasMaxLength(DireccionPublica.LargoMaximo)
            .IsRequired();

        builder.Property(p => p.Nombre)
            .HasMaxLength(PaginaPublica.MaxNombre)
            .IsRequired();

        builder.Property(p => p.Bio).HasMaxLength(PaginaPublica.MaxBio);
        builder.Property(p => p.Instagram).HasMaxLength(PaginaPublica.MaxInstagram);
        builder.Property(p => p.WhatsApp).HasMaxLength(PaginaPublica.MaxWhatsApp);
        builder.Property(p => p.Sobre).HasMaxLength(PaginaPublica.MaxSobre);
        builder.Property(p => p.Ubicacion).HasMaxLength(PaginaPublica.MaxUbicacion);
        builder.Property(p => p.Horarios).HasMaxLength(PaginaPublica.MaxHorarios);

        // R-16: cada foto son tres columnas que admiten null (Foto_Id, Foto_Ancho, Foto_Alto). Es un tipo complejo
        // opcional: sin foto, las tres quedan en null.
        builder.ComplexProperty(p => p.Foto, MapearFoto);
        builder.ComplexProperty(p => p.FotoSobre, MapearFoto);

        builder.Property(p => p.Visible).IsRequired();
        builder.Property(p => p.CreadaEn).IsRequired();
        builder.Property(p => p.ActualizadaEn).IsRequired();

        builder.Property<byte[]>(ColumnaVersion).IsRowVersion();

        // RN-BLG-07: la dirección es única en toda la plataforma, no solo dentro del estudio.
        builder.HasIndex(p => p.Direccion).IsUnique();

        // Una sola página por artista. El filtro deja afuera las páginas de estudio, que no tienen artista.
        builder.HasIndex(p => p.TatuadorId)
            .IsUnique()
            .HasFilter("[TatuadorId] IS NOT NULL");

        // Una sola página propia por estudio: la que no tiene artista.
        builder.HasIndex(p => p.EstudioId)
            .IsUnique()
            .HasFilter("[TatuadorId] IS NULL");
    }

    // Las propiedades no tienen setter: EF las mapea solo si se nombran acá.
    private static void MapearFoto(ComplexPropertyBuilder<FotoPagina> foto)
    {
        foto.Property(f => f.Id);
        foto.Property(f => f.Ancho);
        foto.Property(f => f.Alto);
    }
}
