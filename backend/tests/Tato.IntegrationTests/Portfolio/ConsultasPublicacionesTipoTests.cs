using Tato.Application.Comun;
using Tato.Application.Portfolio;
using Tato.Domain.Portfolio;
using Tato.Infrastructure.Portfolio;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// El tipo de publicación en las consultas del panel contra SQL Server: se puede filtrar y viaja en el resumen
/// y en el detalle (D-50).
/// </summary>
public sealed class ConsultasPublicacionesTipoTests : IClassFixture<BaseDeDatosFixture>
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 16, 12, 0, 0, TimeSpan.Zero);

    private readonly BaseDeDatosFixture _baseDeDatos;

    // xUnit crea una instancia nueva por test: cada test usa su propio estudio.
    private readonly Guid _estudioId = Guid.NewGuid();
    private readonly Guid _usuarioId = Guid.NewGuid();

    public ConsultasPublicacionesTipoTests(BaseDeDatosFixture baseDeDatos)
    {
        _baseDeDatos = baseDeDatos;
    }

    [Fact]
    public async Task Listar_FiltradaPorTipo_SoloDevuelveEseTipo()
    {
        // Arrange
        var trabajo = Crear("Rosa en el antebrazo", TipoPublicacion.Trabajo);
        var blog = Crear("Mi viaje a Bolivia", TipoPublicacion.Blog);
        await GuardarAsync(trabajo, blog);

        // Act
        var pagina = await ListarAsync(new FiltroPublicaciones(null, new Paginacion(), TipoPublicacion.Blog));

        // Assert
        var unica = Assert.Single(pagina.Items);
        Assert.Equal(blog.Id, unica.Id);
        Assert.Equal(TipoPublicacion.Blog, unica.Tipo);
    }

    [Fact]
    public async Task Listar_SinFiltroDeTipo_TraeLosDosTipos()
    {
        // Arrange
        await GuardarAsync(Crear("Un trabajo", TipoPublicacion.Trabajo), Crear("Una del blog", TipoPublicacion.Blog));

        // Act
        var pagina = await ListarAsync(new FiltroPublicaciones(null, new Paginacion()));

        // Assert
        Assert.Equal(2, pagina.Total);
    }

    [Fact]
    public async Task Obtener_DevuelveElTipo()
    {
        // Arrange
        var blog = Crear("Mi viaje a Bolivia", TipoPublicacion.Blog);
        await GuardarAsync(blog);

        // Act
        var detalle = await ObtenerAsync(blog.Id);

        // Assert
        Assert.NotNull(detalle);
        Assert.Equal(TipoPublicacion.Blog, detalle.Tipo);
    }

    private Publicacion Crear(string titulo, TipoPublicacion tipo) =>
        Publicacion.Crear(_estudioId, _usuarioId, new DatosPublicacion(titulo, Tipo: tipo), Ahora);

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
