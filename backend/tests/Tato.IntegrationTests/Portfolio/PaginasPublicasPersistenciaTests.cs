using Microsoft.EntityFrameworkCore;
using Tato.Domain.Portfolio;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// Páginas públicas contra SQL Server: el mapeo de PaginaPublicaConfiguration, la dirección guardada como texto
/// y los índices únicos que protegen las reglas aunque dos pedidos lleguen a la vez (RN-BLG-07).
/// </summary>
public sealed class PaginasPublicasPersistenciaTests : IClassFixture<BaseDeDatosFixture>
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 16, 12, 0, 0, TimeSpan.Zero);

    private readonly BaseDeDatosFixture _baseDeDatos;

    // xUnit crea una instancia nueva por test: cada test usa su propio estudio.
    private readonly Guid _estudioId = Guid.NewGuid();

    public PaginasPublicasPersistenciaTests(BaseDeDatosFixture baseDeDatos)
    {
        _baseDeDatos = baseDeDatos;
    }

    [Fact]
    public async Task Guardar_PaginaDeArtista_SeLeeIgual()
    {
        // Arrange
        var datos = new DatosPagina(
            NombreUnico(),
            Bio: "Blackwork · Ornamental · Freehand",
            Instagram: "@ink.mortall",
            WhatsApp: "+54 9 221 555-0000",
            Sobre: "Tatúo en La Plata.");
        var pagina = PaginaPublica.CrearDeArtista(_estudioId, Guid.NewGuid(), datos, Ahora);

        // Act
        await GuardarAsync(_estudioId, pagina);

        // Assert: se lee desde un contexto nuevo, así los datos salen de la base y no de memoria.
        await using var contexto = _baseDeDatos.CrearContexto(_estudioId);
        var leida = await contexto.PaginasPublicas.SingleAsync(p => p.Id == pagina.Id);

        Assert.Equal(pagina.Direccion, leida.Direccion);
        Assert.Equal(pagina.TatuadorId, leida.TatuadorId);
        Assert.Equal("Blackwork · Ornamental · Freehand", leida.Bio);
        Assert.Equal("ink.mortall", leida.Instagram);
        Assert.Equal("+5492215550000", leida.WhatsApp);
        Assert.False(leida.Visible);
    }

    [Fact]
    public async Task Buscar_PorDireccion_LaEncuentra()
    {
        // Arrange
        var pagina = PaginaPublica.CrearDelEstudio(_estudioId, new DatosPagina(NombreUnico()), Ahora);
        await GuardarAsync(_estudioId, pagina);

        // Act: EF convierte la dirección a texto también en el WHERE.
        await using var contexto = _baseDeDatos.CrearContexto(_estudioId);
        var encontrada = await contexto.PaginasPublicas.SingleOrDefaultAsync(p => p.Direccion == pagina.Direccion);

        // Assert
        Assert.NotNull(encontrada);
        Assert.Equal(pagina.Id, encontrada.Id);
    }

    [Fact]
    public async Task Guardar_LaMismaDireccionEnOtroEstudio_LaBaseLoRechaza()
    {
        // Arrange: el mismo nombre arma la misma dirección.
        var nombre = NombreUnico();
        await GuardarAsync(_estudioId, PaginaPublica.CrearDelEstudio(_estudioId, new DatosPagina(nombre), Ahora));

        var otroEstudio = Guid.NewGuid();
        var repetida = PaginaPublica.CrearDelEstudio(otroEstudio, new DatosPagina(nombre), Ahora);

        // Act & Assert: la dirección es única en toda la plataforma, no solo en el estudio.
        await Assert.ThrowsAsync<DbUpdateException>(() => GuardarAsync(otroEstudio, repetida));
    }

    [Fact]
    public async Task Guardar_DosPaginasDelMismoArtista_LaBaseLoRechaza()
    {
        // Arrange
        var tatuadorId = Guid.NewGuid();
        await GuardarAsync(_estudioId, PaginaDeArtista(tatuadorId));

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => GuardarAsync(_estudioId, PaginaDeArtista(tatuadorId)));
    }

    [Fact]
    public async Task Guardar_DosPaginasDelEstudio_LaBaseLoRechaza()
    {
        // Arrange
        await GuardarAsync(_estudioId, PaginaPublica.CrearDelEstudio(_estudioId, new DatosPagina(NombreUnico()), Ahora));
        var segunda = PaginaPublica.CrearDelEstudio(_estudioId, new DatosPagina(NombreUnico()), Ahora);

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => GuardarAsync(_estudioId, segunda));
    }

    [Fact]
    public async Task Guardar_LaDelEstudioYLasDeDosArtistas_Puede()
    {
        // Arrange: como el estudio piloto, con su página y la de cada artista.
        var paginas = new[]
        {
            PaginaPublica.CrearDelEstudio(_estudioId, new DatosPagina(NombreUnico()), Ahora),
            PaginaDeArtista(Guid.NewGuid()),
            PaginaDeArtista(Guid.NewGuid()),
        };

        // Act
        await GuardarAsync(_estudioId, paginas);

        // Assert
        await using var contexto = _baseDeDatos.CrearContexto(_estudioId);
        Assert.Equal(3, await contexto.PaginasPublicas.CountAsync());
    }

    [Fact]
    public async Task Leer_ConSesionDeOtroEstudio_NoLaVe()
    {
        // Arrange
        await GuardarAsync(_estudioId, PaginaPublica.CrearDelEstudio(_estudioId, new DatosPagina(NombreUnico()), Ahora));

        // Act
        await using var contexto = _baseDeDatos.CrearContexto(Guid.NewGuid());
        var cantidad = await contexto.PaginasPublicas.CountAsync();

        // Assert: el filtro por estudio (D-42) también cubre las páginas.
        Assert.Equal(0, cantidad);
    }

    [Fact]
    public async Task Guardar_ConSusDosFotos_SeLeenIgual()
    {
        // Arrange
        var pagina = PaginaDeArtista(Guid.NewGuid());
        var foto = FotoPagina.Crear(400, 400);
        var sobre = FotoPagina.Crear(1080, 1350);
        pagina.CambiarFoto(LugarFotoPagina.Encabezado, foto, Ahora);
        pagina.CambiarFoto(LugarFotoPagina.Sobre, sobre, Ahora);

        // Act
        await GuardarAsync(_estudioId, pagina);

        // Assert
        await using var contexto = _baseDeDatos.CrearContexto(_estudioId);
        var leida = await contexto.PaginasPublicas.SingleAsync(p => p.Id == pagina.Id);

        Assert.Equal(foto, leida.Foto);
        Assert.Equal(sobre, leida.FotoSobre);
    }

    [Fact]
    public async Task Guardar_SinFotos_LasLeeComoNull()
    {
        // Arrange
        var pagina = PaginaDeArtista(Guid.NewGuid());

        // Act
        await GuardarAsync(_estudioId, pagina);

        // Assert
        await using var contexto = _baseDeDatos.CrearContexto(_estudioId);
        var leida = await contexto.PaginasPublicas.SingleAsync(p => p.Id == pagina.Id);

        Assert.Null(leida.Foto);
        Assert.Null(leida.FotoSobre);
    }

    [Fact]
    public async Task QuitarFoto_YGuardar_LaDejaEnNull()
    {
        // Arrange
        var pagina = PaginaDeArtista(Guid.NewGuid());
        pagina.CambiarFoto(LugarFotoPagina.Encabezado, FotoPagina.Crear(400, 400), Ahora);
        await GuardarAsync(_estudioId, pagina);

        // Act: se quita desde otro contexto, como en otro pedido.
        await using (var contexto = _baseDeDatos.CrearContexto(_estudioId))
        {
            var guardada = await contexto.PaginasPublicas.SingleAsync(p => p.Id == pagina.Id);
            guardada.QuitarFoto(LugarFotoPagina.Encabezado, Ahora);
            await contexto.SaveChangesAsync();
        }

        // Assert
        await using var lectura = _baseDeDatos.CrearContexto(_estudioId);
        var leida = await lectura.PaginasPublicas.SingleAsync(p => p.Id == pagina.Id);

        Assert.Null(leida.Foto);
    }

    /// <summary>
    /// Todos los tests de la clase comparten la base: cada página necesita un nombre, y una dirección, únicos.
    /// </summary>
    private static string NombreUnico() => $"Pagina {Guid.NewGuid():N}";

    private PaginaPublica PaginaDeArtista(Guid tatuadorId) =>
        PaginaPublica.CrearDeArtista(_estudioId, tatuadorId, new DatosPagina(NombreUnico()), Ahora);

    private async Task GuardarAsync(Guid estudioId, params PaginaPublica[] paginas)
    {
        await using var contexto = _baseDeDatos.CrearContexto(estudioId);
        contexto.PaginasPublicas.AddRange(paginas);
        await contexto.SaveChangesAsync();
    }
}
