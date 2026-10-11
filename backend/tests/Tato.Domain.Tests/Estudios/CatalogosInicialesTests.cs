namespace Tato.Domain.Tests.Estudios;

using Tato.Domain.Estudios;
using Xunit;

public class CatalogosInicialesTests
{
    [Fact]
    public void Estilos_NoTienenNombresRepetidosNiInvalidos()
    {
        // Los nombres son únicos por estudio sin distinguir mayúsculas, como la base (R-11).
        Assert.NotEmpty(CatalogosIniciales.Estilos);
        Assert.Equal(
            CatalogosIniciales.Estilos.Count,
            CatalogosIniciales.Estilos.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(CatalogosIniciales.Estilos, nombre => Assert.InRange(nombre.Length, 1, ElementoCatalogo.MaxNombre));
    }

    [Fact]
    public void ZonasCuerpo_NoTienenNombresRepetidosNiInvalidos()
    {
        Assert.NotEmpty(CatalogosIniciales.ZonasCuerpo);
        Assert.Equal(
            CatalogosIniciales.ZonasCuerpo.Count,
            CatalogosIniciales.ZonasCuerpo.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(
            CatalogosIniciales.ZonasCuerpo, nombre => Assert.InRange(nombre.Length, 1, ElementoCatalogo.MaxNombre));
    }

    [Fact]
    public void CrearEstilos_DevuelveUnEstiloActivoPorNombre_DelEstudio()
    {
        // Arrange
        var estudioId = Guid.NewGuid();

        // Act
        var estilos = CatalogosIniciales.CrearEstilos(estudioId);

        // Assert
        Assert.Equal(CatalogosIniciales.Estilos, estilos.Select(e => e.Nombre));
        Assert.All(estilos, e =>
        {
            Assert.Equal(estudioId, e.EstudioId);
            Assert.True(e.Activo);
        });
    }

    [Fact]
    public void CrearZonasCuerpo_DevuelveUnaZonaActivaPorNombre_DelEstudio()
    {
        // Arrange
        var estudioId = Guid.NewGuid();

        // Act
        var zonas = CatalogosIniciales.CrearZonasCuerpo(estudioId);

        // Assert
        Assert.Equal(CatalogosIniciales.ZonasCuerpo, zonas.Select(z => z.Nombre));
        Assert.All(zonas, z =>
        {
            Assert.Equal(estudioId, z.EstudioId);
            Assert.True(z.Activo);
        });
    }

    [Fact]
    public void CrearEstilos_DosEstudios_TienenIdsDistintos()
    {
        // Cada estudio tiene su propio catálogo: nada se comparte entre tenants (D-42).
        var deUno = CatalogosIniciales.CrearEstilos(Guid.NewGuid());
        var deOtro = CatalogosIniciales.CrearEstilos(Guid.NewGuid());

        Assert.Empty(deUno.Select(e => e.Id).Intersect(deOtro.Select(e => e.Id)));
    }
}
