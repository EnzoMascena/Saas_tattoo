using Tato.Application.Comun;
using Tato.Application.Portfolio;
using Tato.Domain.Portfolio;
using Tato.Infrastructure.Portfolio;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// Consultas del panel contra SQL Server: orden, filtro por estado, paginación, portada, versión y aislamiento.
/// </summary>
public sealed class ConsultasPublicacionesTests : IClassFixture<BaseDeDatosFixture>
{
    private const int MaxFotos = 10;

    private static readonly DateTimeOffset Ahora = new(2026, 10, 11, 15, 0, 0, TimeSpan.Zero);

    private readonly BaseDeDatosFixture _baseDeDatos;

    // xUnit crea una instancia nueva por test: cada test usa su propio estudio.
    private readonly Guid _estudioId = Guid.NewGuid();
    private readonly Guid _usuarioId = Guid.NewGuid();

    public ConsultasPublicacionesTests(BaseDeDatosFixture baseDeDatos)
    {
        _baseDeDatos = baseDeDatos;
    }

    [Fact]
    public async Task Listar_SinFiltro_DevuelveTodasDeLaMasRecienteALaMasVieja()
    {
        // Arrange: creadas una hora después de la otra.
        var vieja = Crear("Vieja", Ahora);
        var media = Crear("Media", Ahora.AddHours(1));
        var nueva = Crear("Nueva", Ahora.AddHours(2));
        await GuardarAsync(vieja, media, nueva);

        // Act
        var pagina = await ListarAsync(new FiltroPublicaciones(null, new Paginacion()));

        // Assert
        Assert.Equal(new[] { nueva.Id, media.Id, vieja.Id }, pagina.Items.Select(p => p.Id));
        Assert.Equal(3, pagina.Total);
    }

    [Fact]
    public async Task Listar_FiltradaPorEstado_SoloDevuelveEseEstado()
    {
        // Arrange
        var borrador = Crear("Borrador", Ahora);
        var publicada = Crear("Publicada", Ahora, fotos: 1);
        publicada.ConfirmarConsentimiento(_usuarioId, Ahora);
        publicada.Publicar(Ahora);
        await GuardarAsync(borrador, publicada);

        // Act
        var pagina = await ListarAsync(
            new FiltroPublicaciones(EstadoPublicacion.Publicada, new Paginacion()));

        // Assert
        var unica = Assert.Single(pagina.Items);
        Assert.Equal(publicada.Id, unica.Id);
        Assert.Equal(EstadoPublicacion.Publicada, unica.Estado);
        Assert.Equal(1, pagina.Total);
    }

    [Fact]
    public async Task Listar_ConPaginacion_DevuelveLaPaginaPedidaYElTotal()
    {
        // Arrange: 5 publicaciones; la de i = 4 es la más reciente.
        var publicaciones = Enumerable.Range(0, 5)
            .Select(i => Crear($"Trabajo {i}", Ahora.AddHours(i)))
            .ToArray();
        await GuardarAsync(publicaciones);

        // Act: página 2 de a 2, o sea la 3.ª y la 4.ª más recientes.
        var pagina = await ListarAsync(new FiltroPublicaciones(null, new Paginacion(2, 2)));

        // Assert
        Assert.Equal(new[] { publicaciones[2].Id, publicaciones[1].Id }, pagina.Items.Select(p => p.Id));
        Assert.Equal(5, pagina.Total);
        Assert.Equal(3, pagina.TotalPaginas);
    }

    [Fact]
    public async Task Listar_TraeLaPortadaYLaCantidadDeFotos()
    {
        // Arrange: la tercera foto pasa a ser la portada.
        var conFotos = Crear("Con fotos", Ahora, fotos: 3);
        var tercera = conFotos.Fotos[2];
        conFotos.ElegirPortada(tercera.Id, Ahora);
        var sinFotos = Crear("Sin fotos", Ahora.AddHours(1));
        await GuardarAsync(conFotos, sinFotos);

        // Act
        var pagina = await ListarAsync(new FiltroPublicaciones(null, new Paginacion()));

        // Assert
        var resumenConFotos = Assert.Single(pagina.Items, p => p.Id == conFotos.Id);
        Assert.Equal(tercera.Id, resumenConFotos.PortadaFotoId);
        Assert.Equal(3, resumenConFotos.CantidadFotos);

        var resumenSinFotos = Assert.Single(pagina.Items, p => p.Id == sinFotos.Id);
        Assert.Null(resumenSinFotos.PortadaFotoId);
        Assert.Equal(0, resumenSinFotos.CantidadFotos);
    }

    [Fact]
    public async Task Listar_ConSesionDeOtroEstudio_NoTraeNada()
    {
        await GuardarAsync(Crear("Del estudio", Ahora));

        await using var contexto = _baseDeDatos.CrearContexto(Guid.NewGuid());
        var pagina = await new ConsultasPublicaciones(contexto)
            .ListarAsync(new FiltroPublicaciones(null, new Paginacion()), CancellationToken.None);

        Assert.Empty(pagina.Items);
        Assert.Equal(0, pagina.Total);
    }

    [Fact]
    public async Task Obtener_DevuelveLaPublicacionConSusFotosEnOrden()
    {
        // Arrange
        var publicacion = Crear("Rosa en el antebrazo", Ahora, fotos: 2);
        var ordenEsperado = publicacion.Fotos.Select(f => f.Id).ToList();
        await GuardarAsync(publicacion);

        // Act
        var detalle = await ObtenerAsync(publicacion.Id);

        // Assert
        Assert.NotNull(detalle);
        Assert.Equal("Rosa en el antebrazo", detalle.Titulo);
        Assert.Equal(publicacion.Descripcion, detalle.Descripcion);
        Assert.Equal(EstadoPublicacion.Borrador, detalle.Estado);
        Assert.False(detalle.TieneConsentimiento);
        Assert.Equal(ordenEsperado, detalle.Fotos.Select(f => f.Id));
        Assert.Equal(new[] { 0, 1 }, detalle.Fotos.Select(f => f.Orden));
        Assert.NotEmpty(detalle.Version);
    }

    [Fact]
    public async Task Obtener_DespuesDeUnCambio_LaVersionEsOtra()
    {
        // Arrange
        var publicacion = Crear("Rosa", Ahora);
        await GuardarAsync(publicacion);
        var antes = (await ObtenerAsync(publicacion.Id))!.Version;

        // Act: alguien la edita.
        await using (var contexto = _baseDeDatos.CrearContexto(_estudioId))
        {
            var cargada = await contexto.Publicaciones.FindAsync(publicacion.Id);
            cargada!.Editar(new DatosPublicacion("Rosa y daga"), Ahora.AddHours(1));
            await contexto.SaveChangesAsync();
        }

        // Assert: con la versión vieja, el panel se va a dar cuenta de que hubo un cambio.
        var despues = (await ObtenerAsync(publicacion.Id))!.Version;
        Assert.NotEqual(antes, despues);
    }

    [Fact]
    public async Task Obtener_DeOtroEstudio_DevuelveNull()
    {
        var publicacion = Crear("Del estudio", Ahora);
        await GuardarAsync(publicacion);

        await using var contexto = _baseDeDatos.CrearContexto(Guid.NewGuid());
        var detalle = await new ConsultasPublicaciones(contexto)
            .ObtenerAsync(publicacion.Id, CancellationToken.None);

        Assert.Null(detalle);
    }

    private Publicacion Crear(string titulo, DateTimeOffset creadaEn, int fotos = 0)
    {
        var publicacion = Publicacion.Crear(
            _estudioId, _usuarioId, new DatosPublicacion(titulo, "Descripción de prueba."), creadaEn);

        for (var i = 0; i < fotos; i++)
            publicacion.AgregarFoto(1600, 1200, MaxFotos, creadaEn);

        return publicacion;
    }

    private async Task GuardarAsync(params Publicacion[] publicaciones)
    {
        await using var contexto = _baseDeDatos.CrearContexto(_estudioId);
        contexto.Publicaciones.AddRange(publicaciones);
        await contexto.SaveChangesAsync();
    }

    private async Task<Pagina<PublicacionResumen>> ListarAsync(FiltroPublicaciones filtro)
    {
        await using var contexto = _baseDeDatos.CrearContexto(_estudioId);
        return await new ConsultasPublicaciones(contexto).ListarAsync(filtro, CancellationToken.None);
    }

    private async Task<PublicacionDetalle?> ObtenerAsync(Guid id)
    {
        await using var contexto = _baseDeDatos.CrearContexto(_estudioId);
        return await new ConsultasPublicaciones(contexto).ObtenerAsync(id, CancellationToken.None);
    }
}
