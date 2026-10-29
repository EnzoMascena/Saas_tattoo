using Tato.Application.Comun;
using Tato.Application.Portfolio;
using Tato.Domain.Portfolio;
using Tato.Infrastructure.Portfolio;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// Lo que ve un visitante del sitio, contra SQL Server y sin sesión. Las consultas saltean el filtro por estudio
/// (D-42), así que estos tests prueban sobre todo lo que no se tiene que ver: borradores, ocultas, publicaciones de
/// otra página, de otro estudio y de páginas ocultas (RN-BLG-01, RN-BLG-12).
/// </summary>
public sealed class ConsultasDelSitioTests : IClassFixture<BaseDeDatosFixture>
{
    private const int LargoExtracto = 40;

    private readonly BaseDeDatosFixture _baseDeDatos;

    public ConsultasDelSitioTests(BaseDeDatosFixture baseDeDatos)
    {
        _baseDeDatos = baseDeDatos;
    }

    [Fact]
    public async Task ObtenerPagina_DelEstudio_TraeSusDatosYSoloSusArtistasVisibles()
    {
        // Arrange: una artista con la página visible y con foto, y otra con la página oculta.
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var (_, delEstudio) = await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        var belen = duenio.Como(await duenio.CrearArtistaAsync());
        var (idDeBelen, deBelen) = await belen.CrearPaginaVisibleAsync(delEstudio: false, NombreUnico("Ink Mortal"));
        var foto = await belen.SubirFotoPaginaAsync(
            idDeBelen, LugarFotoPagina.Encabezado, (await belen.ObtenerPaginaAsync(idDeBelen)).Version);
        var emma = duenio.Como(await duenio.CrearArtistaAsync());
        await emma.CrearPaginaAsync(delEstudio: false, new DatosPagina(NombreUnico("Emma")));

        // Act
        var pagina = await PaginaAsync(delEstudio);

        // Assert
        Assert.NotNull(pagina);
        Assert.True(pagina.EsDelEstudio);
        Assert.Equal(delEstudio.Valor, pagina.Direccion);
        Assert.Null(pagina.Estudio);
        var artista = Assert.Single(pagina.Artistas);
        Assert.Equal(deBelen.Valor, artista.Direccion);
        Assert.Equal(foto.Foto, artista.Foto);
    }

    [Fact]
    public async Task ObtenerPagina_DeUnaArtista_TraeSuEstudioSoloSiEstaVisible()
    {
        // Arrange
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var (idDelEstudio, delEstudio) = await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        var artista = duenio.Como(await duenio.CrearArtistaAsync());
        var (_, suya) = await artista.CrearPaginaVisibleAsync(delEstudio: false, NombreUnico("Ink Mortal"));

        // Act & Assert: con la del estudio visible, la enlaza.
        var pagina = await PaginaAsync(suya);
        Assert.NotNull(pagina);
        Assert.False(pagina.EsDelEstudio);
        Assert.Equal(delEstudio.Valor, pagina.Estudio?.Direccion);
        Assert.Empty(pagina.Artistas);

        // Act & Assert: con la del estudio oculta, no.
        await duenio.OcultarPaginaAsync(idDelEstudio, (await duenio.ObtenerPaginaAsync(idDelEstudio)).Version);
        Assert.Null((await PaginaAsync(suya))?.Estudio);
    }

    [Fact]
    public async Task ObtenerPagina_OcultaOQueNoExiste_DevuelveNull()
    {
        // Arrange: creada pero nunca mostrada.
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var id = await duenio.CrearPaginaAsync(delEstudio: true, new DatosPagina(NombreUnico("Alkimya")));
        var oculta = DireccionPublica.Crear((await duenio.ObtenerPaginaAsync(id)).Direccion);

        // Act & Assert
        Assert.Null(await PaginaAsync(oculta));
        Assert.Null(await PaginaAsync(DireccionPublica.Crear($"no-existe-{Guid.NewGuid():N}"[..30])));
    }

    [Fact]
    public async Task ListarPublicaciones_TraeSoloLasPublicadasDeEsaPaginaYDeEseTipo()
    {
        // Arrange
        var escenario = await ArmarEstudioConTodoAsync();

        // Act
        var delEstudio = await PublicacionesAsync(escenario.DelEstudio, TipoPublicacion.Trabajo);
        var deLaArtista = await PublicacionesAsync(escenario.DeLaArtista, TipoPublicacion.Trabajo);
        var deOtroEstudio = await PublicacionesAsync(escenario.DeOtroEstudio, TipoPublicacion.Trabajo);

        // Assert: ni el borrador ni la oculta; ni la de la artista en la del estudio, ni al revés; ni las de otro
        // estudio, aunque su página del estudio tampoco tenga artista.
        Assert.Equal([escenario.TrabajoDelEstudio], delEstudio!.Items.Select(p => p.Id));
        Assert.Equal(1, delEstudio.Total);
        Assert.Equal([escenario.TrabajoDeLaArtista], deLaArtista!.Items.Select(p => p.Id));
        Assert.Equal([escenario.TrabajoDeOtroEstudio], deOtroEstudio!.Items.Select(p => p.Id));
    }

    [Fact]
    public async Task ListarPublicaciones_DelBlog_TraeElComienzoDelTextoYLosTrabajosNo()
    {
        // Arrange
        var escenario = await ArmarEstudioConTodoAsync();

        // Act
        var blog = await PublicacionesAsync(escenario.DelEstudio, TipoPublicacion.Blog);
        var trabajos = await PublicacionesAsync(escenario.DelEstudio, TipoPublicacion.Trabajo);

        // Assert
        var publicacion = Assert.Single(blog!.Items);
        Assert.Equal(escenario.BlogDelEstudio, publicacion.Id);
        Assert.Equal("Primer día en La Plata: llegamos con el…", publicacion.Extracto);
        Assert.NotNull(publicacion.Portada);
        Assert.Null(Assert.Single(trabajos!.Items).Extracto);
    }

    [Fact]
    public async Task ListarPublicaciones_DeUnaPaginaOculta_DevuelveNull()
    {
        // Arrange: la página tenía publicaciones publicadas cuando se ocultó.
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var (id, direccion) = await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        var publicada = await duenio.PublicarNuevaAsync(new DatosPublicacion("Publicada"));
        await duenio.OcultarPaginaAsync(id, (await duenio.ObtenerPaginaAsync(id)).Version);

        // Act & Assert
        Assert.Null(await PublicacionesAsync(direccion, TipoPublicacion.Trabajo));
        Assert.Null(await PublicacionAsync(direccion, publicada));
    }

    [Fact]
    public async Task ListarPublicaciones_PrimeroLasFijadasYDespuesLaPublicadaMasRecientemente()
    {
        // Arrange: se publican en este orden, una hora después de la otra.
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var (_, direccion) = await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        var fijadaVieja = await duenio.PublicarNuevaAsync(new DatosPublicacion("Fijada", Destacada: true));
        duenio.Reloj.Avanzar(TimeSpan.FromHours(1));
        var vieja = await duenio.PublicarNuevaAsync(new DatosPublicacion("Vieja"));
        duenio.Reloj.Avanzar(TimeSpan.FromHours(1));
        var nueva = await duenio.PublicarNuevaAsync(new DatosPublicacion("Nueva"));

        // Act
        var lista = await PublicacionesAsync(direccion, TipoPublicacion.Trabajo);

        // Assert (RN-BLG-13)
        Assert.Equal([fijadaVieja, nueva, vieja], lista!.Items.Select(p => p.Id));
    }

    [Fact]
    public async Task ListarPublicaciones_DeAPartes_TraeElTotalYNoRepiteNinguna()
    {
        // Arrange: tres publicadas a la misma hora; el desempate tiene que ser estable.
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var (_, direccion) = await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        var ids = new[]
        {
            await duenio.PublicarNuevaAsync(new DatosPublicacion("Una")),
            await duenio.PublicarNuevaAsync(new DatosPublicacion("Otra")),
            await duenio.PublicarNuevaAsync(new DatosPublicacion("Y otra")),
        };

        // Act
        var primera = await PublicacionesAsync(direccion, TipoPublicacion.Trabajo, new Paginacion(1, 2));
        var segunda = await PublicacionesAsync(direccion, TipoPublicacion.Trabajo, new Paginacion(2, 2));

        // Assert
        Assert.Equal(3, primera!.Total);
        Assert.Equal(2, primera.Items.Count);
        Assert.Single(segunda!.Items);
        Assert.Equal(
            ids.Order(),
            primera.Items.Concat(segunda.Items).Select(p => p.Id).Order());
    }

    [Fact]
    public async Task ObtenerPublicacion_Publicada_TraeSusFotosEnOrdenYSuTexto()
    {
        // Arrange
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var (_, direccion) = await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        var id = await duenio.PublicarNuevaAsync(
            new DatosPublicacion("Espiral", Descripcion: "Blackwork, dos sesiones."), fotos: 2);
        var fotos = (await duenio.ObtenerAsync(id)).Fotos.Select(f => f.Id);

        // Act
        var detalle = await PublicacionAsync(direccion, id);

        // Assert
        Assert.NotNull(detalle);
        Assert.Equal("Espiral", detalle.Titulo);
        Assert.Equal("Blackwork, dos sesiones.", detalle.Descripcion);
        Assert.Equal(TipoPublicacion.Trabajo, detalle.Tipo);
        Assert.Equal(duenio.Reloj.GetUtcNow(), detalle.PublicadaEn);
        Assert.Equal(fotos, detalle.Fotos.Select(f => f.Id));
    }

    [Fact]
    public async Task ObtenerPublicacion_LoQueNoSeVe_DevuelveNull()
    {
        // Arrange
        var e = await ArmarEstudioConTodoAsync();

        // Act & Assert: borrador y oculta, en su propia página.
        Assert.Null(await PublicacionAsync(e.DelEstudio, e.Borrador));
        Assert.Null(await PublicacionAsync(e.DelEstudio, e.Oculta));

        // Act & Assert: publicadas, pero pedidas desde otra página o desde otro estudio.
        Assert.Null(await PublicacionAsync(e.DelEstudio, e.TrabajoDeLaArtista));
        Assert.Null(await PublicacionAsync(e.DeLaArtista, e.TrabajoDelEstudio));
        Assert.Null(await PublicacionAsync(e.DelEstudio, e.TrabajoDeOtroEstudio));
        Assert.Null(await PublicacionAsync(e.DeOtroEstudio, e.TrabajoDelEstudio));

        // Y cada una, en su página, sí.
        Assert.NotNull(await PublicacionAsync(e.DelEstudio, e.TrabajoDelEstudio));
        Assert.NotNull(await PublicacionAsync(e.DeLaArtista, e.TrabajoDeLaArtista));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("Corto.", "Corto.")]
    [InlineData("Una frase que pasa el largo pedido", "Una frase que…")]
    [InlineData("Unapalabrasinespaciosmuylarga", "Unapalabrasinesp…")]
    public void Extracto_CortaEnElUltimoEspacioYAgregaPuntosSuspensivos(string? texto, string? esperado)
    {
        Assert.Equal(esperado, ConsultasDelSitio.Extracto(texto, largo: 16));
    }

    /// <summary>
    /// Un estudio con su página y la de una artista, las dos visibles, con una publicación de cada caso, y otro
    /// estudio con su página y una publicación.
    /// </summary>
    private async Task<Escenario> ArmarEstudioConTodoAsync()
    {
        var duenio = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var artista = duenio.Como(await duenio.CrearArtistaAsync());
        var (_, delEstudio) = await duenio.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Alkimya"));
        var (_, deLaArtista) = await artista.CrearPaginaVisibleAsync(delEstudio: false, NombreUnico("Ink Mortal"));

        var trabajo = await duenio.PublicarNuevaAsync(new DatosPublicacion("Trabajo del estudio"));
        var blog = await duenio.PublicarNuevaAsync(new DatosPublicacion(
            "Viaje",
            Descripcion: "Primer día en La Plata: llegamos con el equipo y armamos el estudio.",
            Tipo: TipoPublicacion.Blog));
        var borrador = await duenio.CrearPublicacionAsync("Borrador");
        var oculta = await duenio.PublicarNuevaAsync(new DatosPublicacion("Oculta"));
        await duenio.OcultarAsync(oculta, (await duenio.ObtenerAsync(oculta)).Version);
        var deArtista = await artista.PublicarNuevaAsync(
            new DatosPublicacion("Trabajo de la artista", TatuadorId: artista.Usuario.TatuadorId));

        var otro = await Panel.CrearAsync(_baseDeDatos, UsuarioFalso.Duenio());
        var (_, deOtroEstudio) = await otro.CrearPaginaVisibleAsync(delEstudio: true, NombreUnico("Otro"));
        var deOtro = await otro.PublicarNuevaAsync(new DatosPublicacion("Trabajo de otro estudio"));

        return new Escenario(
            delEstudio, deLaArtista, deOtroEstudio, trabajo, blog, borrador, oculta, deArtista, deOtro);
    }

    private async Task<PaginaDelSitio?> PaginaAsync(DireccionPublica direccion)
    {
        await using var db = _baseDeDatos.CrearContexto(estudioId: null);
        return await new ConsultasDelSitio(db).ObtenerPaginaAsync(direccion, CancellationToken.None);
    }

    private async Task<Pagina<PublicacionDelSitio>?> PublicacionesAsync(
        DireccionPublica direccion,
        TipoPublicacion tipo,
        Paginacion? paginacion = null)
    {
        await using var db = _baseDeDatos.CrearContexto(estudioId: null);
        var filtro = new FiltroPublicacionesDelSitio(tipo, paginacion ?? new Paginacion(), LargoExtracto);
        return await new ConsultasDelSitio(db).ListarPublicacionesAsync(direccion, filtro, CancellationToken.None);
    }

    private async Task<PublicacionDelSitioDetalle?> PublicacionAsync(DireccionPublica direccion, Guid id)
    {
        await using var db = _baseDeDatos.CrearContexto(estudioId: null);
        return await new ConsultasDelSitio(db).ObtenerPublicacionAsync(direccion, id, CancellationToken.None);
    }

    /// <summary>
    /// Todos los tests de la clase comparten la base: cada página necesita un nombre, y una dirección, únicos.
    /// </summary>
    private static string NombreUnico(string nombre) => $"{nombre} {Guid.NewGuid():N}";

    private sealed record Escenario(
        DireccionPublica DelEstudio,
        DireccionPublica DeLaArtista,
        DireccionPublica DeOtroEstudio,
        Guid TrabajoDelEstudio,
        Guid BlogDelEstudio,
        Guid Borrador,
        Guid Oculta,
        Guid TrabajoDeLaArtista,
        Guid TrabajoDeOtroEstudio);
}
