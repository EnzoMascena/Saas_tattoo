using Microsoft.EntityFrameworkCore;
using Tato.Domain.Estudios;

namespace Tato.IntegrationTests.Estudios;

/// <summary>
/// Catálogos de estilos y zonas del cuerpo (R-11) contra SQL Server: aislamiento por estudio (D-42)
/// y nombres únicos dentro de cada estudio.
/// </summary>
public sealed class CatalogosPersistenciaTests : IClassFixture<BaseDeDatosFixture>
{
    private readonly BaseDeDatosFixture _baseDeDatos;

    public CatalogosPersistenciaTests(BaseDeDatosFixture baseDeDatos)
    {
        _baseDeDatos = baseDeDatos;
    }

    [Fact]
    public async Task Guardar_CatalogosIniciales_SoloLosVeSuEstudio()
    {
        // Arrange: lo mismo que guarda RegistroEstudios en el alta, sin sesión, como la consola.
        var estudio = Estudio.Crear("Estudio con catálogos", "piloto");

        await using (var contexto = _baseDeDatos.CrearContexto(estudioId: null))
        {
            contexto.Estudios.Add(estudio);
            contexto.Estilos.AddRange(CatalogosIniciales.CrearEstilos(estudio.Id));
            contexto.ZonasCuerpo.AddRange(CatalogosIniciales.CrearZonasCuerpo(estudio.Id));
            await contexto.SaveChangesAsync();
        }

        // Act & Assert: con la sesión del estudio ve todos sus valores.
        await using (var delEstudio = _baseDeDatos.CrearContexto(estudio.Id))
        {
            Assert.Equal(CatalogosIniciales.Estilos.Count, await delEstudio.Estilos.CountAsync());
            Assert.Equal(CatalogosIniciales.ZonasCuerpo.Count, await delEstudio.ZonasCuerpo.CountAsync());
        }

        // Con la sesión de otro estudio no ve ninguno.
        await using var deOtro = _baseDeDatos.CrearContexto(Guid.NewGuid());

        Assert.False(await deOtro.Estilos.AnyAsync());
        Assert.False(await deOtro.ZonasCuerpo.AnyAsync());
    }

    [Fact]
    public async Task Guardar_EstiloRepetidoEnElMismoEstudio_LanzaDbUpdateException()
    {
        // La base compara sin distinguir mayúsculas: «Fineline» y «fineline» son el mismo nombre.
        var estudioId = Guid.NewGuid();
        await using var contexto = _baseDeDatos.CrearContexto(estudioId);

        contexto.Estilos.AddRange(Estilo.Crear(estudioId, "Fineline"), Estilo.Crear(estudioId, "fineline"));

        await Assert.ThrowsAsync<DbUpdateException>(() => contexto.SaveChangesAsync());
    }

    [Fact]
    public async Task Guardar_MismoNombreEnOtroEstudio_SePermite()
    {
        // El índice es por estudio: dos estudios distintos pueden tener un estilo con el mismo nombre.
        await using (var deUno = _baseDeDatos.CrearContexto(estudioId: null))
        {
            deUno.Estilos.AddRange(Estilo.Crear(Guid.NewGuid(), "Realismo"), Estilo.Crear(Guid.NewGuid(), "Realismo"));
            await deUno.SaveChangesAsync();
        }
    }
}
