using Tato.Application.Comun;
using Tato.Domain.Comun;
using Tato.Domain.Portfolio;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// Casos de uso de publicaciones contra SQL Server: control de versión, permisos (P-21) y aislamiento (D-42).
/// </summary>
public sealed class PanelPublicacionesTests : IClassFixture<BaseDeDatosFixture>
{
    private readonly BaseDeDatosFixture _baseDeDatos;

    public PanelPublicacionesTests(BaseDeDatosFixture baseDeDatos)
    {
        _baseDeDatos = baseDeDatos;
    }

    [Fact]
    public async Task Crear_GuardaUnBorrador()
    {
        // Arrange
        var panel = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());

        // Act
        var id = await panel.CrearPublicacionAsync("Realismo en antebrazo");

        // Assert
        var detalle = await panel.ObtenerAsync(id);
        Assert.Equal("Realismo en antebrazo", detalle.Titulo);
        Assert.Equal(EstadoPublicacion.Borrador, detalle.Estado);
    }

    [Fact]
    public async Task Crear_ConUnEstiloQueNoExiste_LanzaLaReglaR11()
    {
        // Arrange
        var panel = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var datos = new DatosPublicacion("Con estilo falso", EstiloId: Guid.NewGuid());

        // Act & Assert
        var error = await Assert.ThrowsAsync<ReglaDeNegocioException>(() => panel.CrearPublicacionAsync(datos));
        Assert.Equal("R-11", error.Regla);
    }

    [Fact]
    public async Task Editar_ConLaVersionVigente_GuardaYDevuelveUnaVersionNueva()
    {
        // Arrange
        var panel = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var id = await panel.CrearPublicacionAsync("Título con error");
        var version = (await panel.ObtenerAsync(id)).Version;

        // Act
        var versionNueva = await panel.EditarAsync(id, "Título corregido", version);

        // Assert
        var detalle = await panel.ObtenerAsync(id);
        Assert.Equal("Título corregido", detalle.Titulo);
        Assert.NotEqual(version, versionNueva);
        Assert.Equal(versionNueva, detalle.Version);
    }

    [Fact]
    public async Task Editar_ConUnaVersionVieja_LanzaConflictoYNoPisaElCambioAnterior()
    {
        // Arrange: dos personas abren la publicación al mismo tiempo, con la misma versión.
        var panel = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var id = await panel.CrearPublicacionAsync("Original");
        var versionAlAbrir = (await panel.ObtenerAsync(id)).Version;

        // Act: la primera guarda; la segunda intenta guardar con la versión que tenía al abrir.
        await panel.EditarAsync(id, "Editada por A", versionAlAbrir);

        // Assert
        await Assert.ThrowsAsync<ConflictoDeVersionException>(
            () => panel.EditarAsync(id, "Editada por B", versionAlAbrir));
        Assert.Equal("Editada por A", (await panel.ObtenerAsync(id)).Titulo);
    }

    [Fact]
    public async Task Editar_UnTatuadorEnUnaPublicacionAjena_LanzaAccesoDenegado()
    {
        // Arrange: la crea el Dueño.
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var id = await duenio.CrearPublicacionAsync("Del dueño");
        var version = (await duenio.ObtenerAsync(id)).Version;
        var tatuador = duenio.Como(UsuarioFalso.Tatuador());

        // Act & Assert
        await Assert.ThrowsAsync<AccesoDenegadoException>(() => tatuador.EditarAsync(id, "Intento", version));
    }

    [Fact]
    public async Task Editar_UnTatuadorEnSuPropiaPublicacion_Puede()
    {
        // Arrange: la crea el mismo Tatuador.
        var tatuador = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Tatuador());
        var id = await tatuador.CrearPublicacionAsync("Mi trabajo");
        var version = (await tatuador.ObtenerAsync(id)).Version;

        // Act
        await tatuador.EditarAsync(id, "Mi trabajo, corregido", version);

        // Assert
        Assert.Equal("Mi trabajo, corregido", (await tatuador.ObtenerAsync(id)).Titulo);
    }

    [Fact]
    public async Task Publicar_UnTatuador_LanzaAccesoDenegadoAunqueSeaSuya()
    {
        // Arrange
        var tatuador = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Tatuador());
        var id = await tatuador.CrearPublicacionAsync("Mi trabajo");
        var version = (await tatuador.ObtenerAsync(id)).Version;

        // Act & Assert: el permiso se controla antes que las reglas del dominio (fotos, consentimiento).
        await Assert.ThrowsAsync<AccesoDenegadoException>(() => tatuador.PublicarAsync(id, version));
    }

    [Fact]
    public async Task Editar_UnaPublicacionDeOtroEstudio_LanzaNoEncontrado()
    {
        // Arrange: la publicación es del estudio A.
        var estudioA = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var id = await estudioA.CrearPublicacionAsync("Del estudio A");
        var version = (await estudioA.ObtenerAsync(id)).Version;
        var estudioB = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());

        // Act & Assert: para el estudio B no existe, aunque conozca el Id y la versión (D-42).
        await Assert.ThrowsAsync<NoEncontradoException>(() => estudioB.EditarAsync(id, "Intento", version));
    }
}
