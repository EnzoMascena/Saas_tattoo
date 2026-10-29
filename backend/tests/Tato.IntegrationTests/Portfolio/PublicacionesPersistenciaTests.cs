using Microsoft.EntityFrameworkCore;
using Tato.Domain.Portfolio;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// Publicaciones y fotos contra SQL Server: el mapeo de PublicacionConfiguration y FotoConfiguration,
/// la concurrencia optimista a nivel del agregado y el aislamiento por estudio (D-42).
/// </summary>
public sealed class PublicacionesPersistenciaTests : IClassFixture<BaseDeDatosFixture>
{
    private const int MaxFotos = 10;

    private static readonly DateTimeOffset Ahora = new(2026, 10, 11, 15, 0, 0, TimeSpan.Zero);

    private readonly BaseDeDatosFixture _baseDeDatos;

    // xUnit crea una instancia nueva por test: cada test usa su propio estudio.
    private readonly Guid _estudioId = Guid.NewGuid();
    private readonly Guid _usuarioId = Guid.NewGuid();

    public PublicacionesPersistenciaTests(BaseDeDatosFixture baseDeDatos)
    {
        _baseDeDatos = baseDeDatos;
    }

    [Fact]
    public async Task Guardar_PublicacionConFotos_SeLeeIgualYEnOrden()
    {
        // Arrange: la tercera foto pasa a ser la portada, y la publicación queda publicada.
        var publicacion = CrearPublicacion(cantidadFotos: 3);
        var tercera = publicacion.Fotos[2];
        publicacion.ElegirPortada(tercera.Id, Ahora);
        publicacion.ConfirmarConsentimiento(_usuarioId, Ahora);
        publicacion.Publicar(Ahora);
        var ordenEsperado = publicacion.Fotos.Select(f => f.Id).ToList();

        // Act
        await GuardarAsync(publicacion);

        // Assert: se lee desde un contexto nuevo, así los datos salen de la base y no de memoria.
        await using var contexto = _baseDeDatos.CrearContexto(_estudioId);
        var leida = await contexto.Publicaciones
            .Include(p => p.Fotos)
            .SingleAsync(p => p.Id == publicacion.Id);

        Assert.Equal(publicacion.Titulo, leida.Titulo);
        Assert.Equal(publicacion.Descripcion, leida.Descripcion);
        Assert.Equal(publicacion.FechaTrabajo, leida.FechaTrabajo);
        Assert.Equal(publicacion.EstiloId, leida.EstiloId);
        Assert.Equal(EstadoPublicacion.Publicada, leida.Estado);
        Assert.Equal(_usuarioId, leida.ConsentimientoConfirmadoPor);
        Assert.Equal(Ahora, leida.PublicadaEn);
        Assert.Equal(ordenEsperado, leida.Fotos.Select(f => f.Id));
        Assert.Equal(tercera.Id, leida.Portada?.Id);
    }

    [Fact]
    public async Task QuitarFoto_YGuardar_BorraLaFilaYCorreElOrden()
    {
        // Arrange
        var publicacion = CrearPublicacion(cantidadFotos: 2);
        var segunda = publicacion.Fotos[1];
        await GuardarAsync(publicacion);

        // Act: se quita la primera foto desde un contexto nuevo, como haría el panel.
        await using (var contexto = _baseDeDatos.CrearContexto(_estudioId))
        {
            var cargada = await contexto.Publicaciones
                .Include(p => p.Fotos)
                .SingleAsync(p => p.Id == publicacion.Id);

            cargada.QuitarFoto(cargada.Fotos[0].Id, Ahora.AddHours(1));
            await contexto.SaveChangesAsync();
        }

        // Assert: en la base queda solo la segunda, ahora en el lugar 0.
        await using var lectura = _baseDeDatos.CrearContexto(_estudioId);
        var fotos = await lectura.Set<Foto>()
            .Where(f => f.PublicacionId == publicacion.Id)
            .ToListAsync();

        var foto = Assert.Single(fotos);
        Assert.Equal(segunda.Id, foto.Id);
        Assert.Equal(0, foto.Orden);
    }

    [Fact]
    public async Task Guardar_DosEdicionesALaVez_LaSegundaLanzaDbUpdateConcurrencyException()
    {
        // Arrange: dos personas abren la misma publicación.
        var publicacion = CrearPublicacion(cantidadFotos: 1);
        await GuardarAsync(publicacion);

        await using var primera = _baseDeDatos.CrearContexto(_estudioId);
        await using var segunda = _baseDeDatos.CrearContexto(_estudioId);
        var enPrimera = await primera.Publicaciones.SingleAsync(p => p.Id == publicacion.Id);
        var enSegunda = await segunda.Publicaciones.SingleAsync(p => p.Id == publicacion.Id);

        // Act: la primera guarda antes.
        enPrimera.Editar(new DatosPublicacion("Cambio de la primera"), Ahora.AddMinutes(1));
        await primera.SaveChangesAsync();

        // Assert: la segunda trabajaba con una versión vieja y no puede pisar el cambio.
        enSegunda.Editar(new DatosPublicacion("Cambio de la segunda"), Ahora.AddMinutes(2));
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => segunda.SaveChangesAsync());
    }

    [Fact]
    public async Task AgregarFoto_MientrasOtroEdita_LaEdicionLanzaDbUpdateConcurrencyException()
    {
        // Agregar una foto también cambia la publicación (ActualizadaEn), así que el rowversion protege
        // todo el agregado y no solo los datos de la publicación.
        var publicacion = CrearPublicacion(cantidadFotos: 1);
        await GuardarAsync(publicacion);

        await using var conFoto = _baseDeDatos.CrearContexto(_estudioId);
        await using var conEdicion = _baseDeDatos.CrearContexto(_estudioId);
        var paraFoto = await conFoto.Publicaciones
            .Include(p => p.Fotos)
            .SingleAsync(p => p.Id == publicacion.Id);
        var paraEdicion = await conEdicion.Publicaciones.SingleAsync(p => p.Id == publicacion.Id);

        paraFoto.AgregarFoto(1200, 1600, MaxFotos, Ahora.AddMinutes(1));
        await conFoto.SaveChangesAsync();

        paraEdicion.Editar(new DatosPublicacion("Título nuevo"), Ahora.AddMinutes(2));
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => conEdicion.SaveChangesAsync());
    }

    [Fact]
    public async Task Consultar_ConSesionDeOtroEstudio_NoVeLaPublicacionNiSusFotos()
    {
        // Arrange
        var publicacion = CrearPublicacion(cantidadFotos: 2);
        await GuardarAsync(publicacion);

        // Act & Assert
        await using var contexto = _baseDeDatos.CrearContexto(Guid.NewGuid());

        Assert.False(await contexto.Publicaciones.AnyAsync(p => p.Id == publicacion.Id));
        Assert.False(await contexto.Set<Foto>().AnyAsync(f => f.PublicacionId == publicacion.Id));
    }

    private Publicacion CrearPublicacion(int cantidadFotos)
    {
        var datos = new DatosPublicacion(
            "Rosa en el antebrazo",
            "Fineline con sombras suaves.",
            new DateOnly(2026, 9, 20),
            EstiloId: Guid.NewGuid());

        var publicacion = Publicacion.Crear(_estudioId, _usuarioId, datos, Ahora);

        for (var i = 0; i < cantidadFotos; i++)
            publicacion.AgregarFoto(1600, 1200, MaxFotos, Ahora);

        return publicacion;
    }

    private async Task GuardarAsync(Publicacion publicacion)
    {
        await using var contexto = _baseDeDatos.CrearContexto(_estudioId);
        contexto.Publicaciones.Add(publicacion);
        await contexto.SaveChangesAsync();
    }
}
