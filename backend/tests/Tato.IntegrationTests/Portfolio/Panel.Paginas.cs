using Tato.Application.Portfolio;
using Tato.Domain.Portfolio;
using Tato.Infrastructure.Persistencia;
using Tato.Infrastructure.Portfolio;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// La pantalla «Mi página» del panel: los casos de uso de las páginas públicas, armados como en la API. Igual que
/// con las publicaciones, cada operación usa un contexto nuevo, como cada pedido HTTP.
/// </summary>
internal sealed partial class Panel
{
    public async Task<Guid> CrearPaginaAsync(bool delEstudio, DatosPagina datos)
    {
        await using var db = Contexto();
        var crear = new CrearPagina(new RepositorioPaginas(db), _estudio, Usuario, Reloj);
        return await crear.EjecutarAsync(delEstudio, datos);
    }

    public async Task<IReadOnlyList<PaginaDetalle>> MisPaginasAsync()
    {
        await using var db = Contexto();
        return await new MisPaginas(new ConsultasPaginas(db), Usuario).EjecutarAsync();
    }

    public async Task<string> EditarPaginaAsync(Guid id, DatosPagina datos, string version)
    {
        await using var db = Contexto();
        return await Acciones(db).EditarAsync(id, datos, version);
    }

    public async Task<string> CambiarDireccionAsync(Guid id, string direccion, string version)
    {
        await using var db = Contexto();
        return await Acciones(db).CambiarDireccionAsync(id, direccion, version);
    }

    public async Task<string> MostrarPaginaAsync(Guid id, string version)
    {
        await using var db = Contexto();
        return await Acciones(db).MostrarAsync(id, version);
    }

    public async Task<string> OcultarPaginaAsync(Guid id, string version)
    {
        await using var db = Contexto();
        return await Acciones(db).OcultarAsync(id, version);
    }

    /// <summary>
    /// Guarda la página del estudio directo con el repositorio, sin los controles del caso de uso: para probar que
    /// la base también rechaza una dirección repetida.
    /// </summary>
    public async Task AgregarPaginaSinControlesAsync(DatosPagina datos)
    {
        await using var db = Contexto();
        var pagina = PaginaPublica.CrearDelEstudio(EstudioId, datos, Reloj.GetUtcNow());
        await new RepositorioPaginas(db).AgregarAsync(pagina, CancellationToken.None);
    }

    /// <summary>
    /// Lee la página desde la base, con un contexto nuevo: lo que devuelve es lo que quedó guardado.
    /// </summary>
    public async Task<PaginaDetalle> ObtenerPaginaAsync(Guid id)
    {
        await using var db = Contexto();
        return await new ConsultasPaginas(db).ObtenerAsync(id, CancellationToken.None)
            ?? throw new InvalidOperationException($"La página {id} no existe en el estudio {EstudioId}.");
    }

    /// <summary>
    /// Sube la foto del encabezado o de «Sobre». Sin procesador, usa uno que la acepta siempre.
    /// </summary>
    public async Task<FotoPaginaSubida> SubirFotoPaginaAsync(
        Guid id,
        LugarFotoPagina lugar,
        string version,
        IProcesadorFotos? procesador = null)
    {
        await using var db = Contexto();
        var subir = new SubirFotoPagina(
            new RepositorioPaginas(db), procesador ?? new ProcesadorFalso(), Almacenamiento, Usuario, Reloj);
        return await subir.EjecutarAsync(id, lugar, Stream.Null, version);
    }

    public async Task<string> QuitarFotoPaginaAsync(Guid id, LugarFotoPagina lugar, string version)
    {
        await using var db = Contexto();
        return await Acciones(db).QuitarFotoAsync(id, lugar, version);
    }

    /// <summary>
    /// Abre una foto como el panel, con el estudio de esta sesión. Null si no la encuentra.
    /// </summary>
    public Task<Stream?> LeerFotoAsync(Guid fotoId, TamanioFoto tamanio) =>
        new LeerFoto(_estudio, Almacenamiento).EjecutarAsync(fotoId, tamanio);

    private AccionesPagina Acciones(TatoDbContext db) =>
        new(new RepositorioPaginas(db), Almacenamiento, Usuario, Reloj);
}
