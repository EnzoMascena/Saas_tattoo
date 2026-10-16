using Microsoft.EntityFrameworkCore;
using Tato.Domain.Comun;
using Tato.Domain.Estudios;
using Tato.Infrastructure.Persistencia;

namespace Tato.IntegrationTests.Estudios;

/// <summary>
/// Aislamiento entre estudios (D-42): un estudio no puede leer ni modificar datos de otro (doc 06, 4 y 7.5).
/// Cada test arranca con dos estudios nuevos, A y B, con un usuario Dueño y Tatuador en cada uno.
/// </summary>
public sealed class AislamientoEntreEstudiosTests : IClassFixture<BaseDeDatosFixture>, IAsyncLifetime
{
    private readonly BaseDeDatosFixture _baseDeDatos;

    private readonly Estudio _estudioA = Estudio.Crear("Estudio A", "piloto");
    private readonly Estudio _estudioB = Estudio.Crear("Estudio B", "piloto");
    private Usuario _usuarioA = null!;
    private Usuario _usuarioB = null!;

    public AislamientoEntreEstudiosTests(BaseDeDatosFixture baseDeDatos)
    {
        _baseDeDatos = baseDeDatos;
    }

    public async Task InitializeAsync()
    {
        _usuarioA = CrearUsuario(_estudioA);
        _usuarioB = CrearUsuario(_estudioB);

        // Sin estudio en la sesión, como los comandos de consola: puede dar de alta en cualquier estudio.
        await using var contexto = _baseDeDatos.CrearContexto(estudioId: null);

        contexto.AddRange(
            _estudioA,
            _estudioB,
            _usuarioA,
            _usuarioB,
            Tatuador.Crear(_usuarioA, "Tatuador A"),
            Tatuador.Crear(_usuarioB, "Tatuador B"));

        await contexto.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Consultar_ConSesionDeUnEstudio_SoloDevuelveSusDatos()
    {
        await using var contexto = _baseDeDatos.CrearContexto(_estudioA.Id);

        var usuario = Assert.Single(await contexto.Usuarios.ToListAsync());
        var tatuador = Assert.Single(await contexto.Tatuadores.ToListAsync());

        Assert.Equal(_usuarioA.Id, usuario.Id);
        Assert.Equal(_usuarioA.Id, tatuador.UsuarioId);
    }

    [Fact]
    public async Task BuscarPorId_DatoDeOtroEstudio_NoLoEncuentra()
    {
        await using var contexto = _baseDeDatos.CrearContexto(_estudioA.Id);

        var usuario = await contexto.Usuarios.SingleOrDefaultAsync(u => u.Id == _usuarioB.Id);

        Assert.Null(usuario);
    }

    [Fact]
    public async Task Consultar_SinEstudioEnLaSesion_NoDevuelveNada()
    {
        await using var contexto = _baseDeDatos.CrearContexto(estudioId: null);

        Assert.False(await contexto.Usuarios.AnyAsync());
        Assert.False(await contexto.Tatuadores.AnyAsync());
    }

    [Fact]
    public async Task IgnorarFiltroEstudio_AProposito_VeLosDosEstudios()
    {
        // Es lo que hace el inicio de sesión, que todavía no tiene estudio.
        await using var contexto = _baseDeDatos.CrearContexto(_estudioA.Id);

        var cantidad = await contexto.Usuarios
            .IgnoreQueryFilters([TatoDbContext.FiltroEstudio])
            .CountAsync(u => u.Id == _usuarioA.Id || u.Id == _usuarioB.Id);

        Assert.Equal(2, cantidad);
    }

    [Fact]
    public async Task Guardar_DatoDeOtroEstudio_LanzaInvalidOperationException()
    {
        await using var contexto = _baseDeDatos.CrearContexto(_estudioA.Id);
        contexto.Usuarios.Add(CrearUsuario(_estudioB));

        await Assert.ThrowsAsync<InvalidOperationException>(() => contexto.SaveChangesAsync());
    }

    [Fact]
    public async Task ModificarEnBloque_DatoDeOtroEstudio_NoLoModifica()
    {
        await using (var contexto = _baseDeDatos.CrearContexto(_estudioA.Id))
        {
            var modificados = await contexto.Usuarios
                .Where(u => u.Id == _usuarioB.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Nombre, "Modificado"));

            Assert.Equal(0, modificados);
        }

        await using var contextoB = _baseDeDatos.CrearContexto(_estudioB.Id);
        var usuarioB = await contextoB.Usuarios.SingleAsync(u => u.Id == _usuarioB.Id);

        Assert.Equal(_usuarioB.Nombre, usuarioB.Nombre);
    }

    [Fact]
    public void Modelo_TodaEntidadConEstudioId_TieneElFiltroPorEstudio()
    {
        // Si alguien agrega una entidad con EstudioId y se olvida de IPerteneceAEstudio, este test falla.
        using var contexto = _baseDeDatos.CrearContexto(estudioId: null);

        var sinFiltro = contexto.Model.GetEntityTypes()
            .Where(t => t.FindProperty(nameof(IPerteneceAEstudio.EstudioId)) is not null)
            .Where(t => t.FindDeclaredQueryFilter(TatoDbContext.FiltroEstudio) is null)
            .Select(t => t.ClrType.Name);

        Assert.Empty(sinFiltro);
    }

    private static Usuario CrearUsuario(Estudio estudio) =>
        Usuario.Crear(estudio.Id, $"{Guid.NewGuid()}@ejemplo.com", "Usuario de prueba", Rol.Duenio | Rol.Tatuador);
}
