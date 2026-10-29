using Tato.Application.Comun;
using Tato.Application.Estudios;
using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Caso de uso: subir la foto o logo del encabezado, o la foto de «Sobre», de una página (R-16). Si ya tenía una, la
/// reemplaza. Puede hacerlo quien gestiona la página (<see cref="PermisosPaginas"/>).
/// <para>
/// Como en <see cref="SubirFoto"/>: primero los archivos y después la base, y si algo falla se borran los archivos
/// nuevos. Los de la foto reemplazada se borran recién después de guardar, así la página nunca apunta a archivos
/// que no existen.
/// </para>
/// </summary>
public sealed class SubirFotoPagina
{
    private readonly IRepositorioPaginas _repositorio;
    private readonly IProcesadorFotos _procesador;
    private readonly IAlmacenamientoArchivos _almacenamiento;
    private readonly IUsuarioActual _usuarioActual;
    private readonly TimeProvider _reloj;

    public SubirFotoPagina(
        IRepositorioPaginas repositorio,
        IProcesadorFotos procesador,
        IAlmacenamientoArchivos almacenamiento,
        IUsuarioActual usuarioActual,
        TimeProvider reloj)
    {
        _repositorio = repositorio;
        _procesador = procesador;
        _almacenamiento = almacenamiento;
        _usuarioActual = usuarioActual;
        _reloj = reloj;
    }

    /// <param name="archivo">La foto tal como llegó del celular. Quien llama la cierra.</param>
    /// <param name="version">La versión que tenía el panel (ver PaginaDetalle.Version).</param>
    public async Task<FotoPaginaSubida> EjecutarAsync(
        Guid paginaId,
        LugarFotoPagina lugar,
        Stream archivo,
        string version,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _repositorio.ObtenerAsync(paginaId, cancellationToken)
            ?? throw new NoEncontradoException("La página no existe.");

        PermisosPaginas.ExigirGestionar(_usuarioActual, pagina.TatuadorId);

        // Procesar antes de guardar nada: si la foto no sirve (400), no queda ningún rastro.
        var procesada = await _procesador.ProcesarAsync(archivo, cancellationToken);
        var foto = FotoPagina.Crear(procesada.Detalle.Ancho, procesada.Detalle.Alto);
        var anterior = pagina.CambiarFoto(lugar, foto, _reloj.GetUtcNow());

        string versionNueva;

        try
        {
            await ArchivosDeFotos.GuardarAsync(
                _almacenamiento,
                procesada,
                tamanio => ClavesFotos.DePagina(pagina.EstudioId, foto.Id, tamanio),
                cancellationToken);

            versionNueva = await _repositorio.GuardarCambiosAsync(pagina, version, cancellationToken);
        }
        catch
        {
            // No se pudo guardar (por ejemplo, 409): la página sigue con la foto anterior y sus archivos.
            await ArchivosDeFotos.BorrarAsync(_almacenamiento, ClavesFotos.TodasDePagina(pagina.EstudioId, foto.Id));
            throw;
        }

        // Si esto falla, quedan archivos que nadie muestra, pero nada se rompe.
        if (anterior is not null)
            await ArchivosDeFotos.BorrarAsync(
                _almacenamiento, ClavesFotos.TodasDePagina(pagina.EstudioId, anterior.Id));

        return new FotoPaginaSubida(new FotoConMedidas(foto.Id, foto.Ancho, foto.Alto), versionNueva);
    }
}
