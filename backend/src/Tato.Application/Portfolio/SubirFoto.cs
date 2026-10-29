using Tato.Application.Comun;
using Tato.Application.Estudios;

namespace Tato.Application.Portfolio;

/// <summary>
/// Caso de uso: subir una foto a una publicación (RN-BLG-04, RN-BLG-05). La procesa, guarda sus versiones y la
/// agrega al final. Puede hacerlo quien puede editar la publicación (D-51).
/// <para>
/// Los archivos y la base no comparten transacción. Por eso primero se guardan los archivos y después la base,
/// y si algo falla se borran los archivos: nunca queda una foto en la base sin sus archivos, ni archivos sueltos.
/// </para>
/// </summary>
public sealed class SubirFoto
{
    private readonly IRepositorioPublicaciones _repositorio;
    private readonly IReferenciasDelEstudio _referencias;
    private readonly IProcesadorFotos _procesador;
    private readonly IAlmacenamientoArchivos _almacenamiento;
    private readonly IUsuarioActual _usuarioActual;
    private readonly TimeProvider _reloj;

    public SubirFoto(
        IRepositorioPublicaciones repositorio,
        IReferenciasDelEstudio referencias,
        IProcesadorFotos procesador,
        IAlmacenamientoArchivos almacenamiento,
        IUsuarioActual usuarioActual,
        TimeProvider reloj)
    {
        _repositorio = repositorio;
        _referencias = referencias;
        _procesador = procesador;
        _almacenamiento = almacenamiento;
        _usuarioActual = usuarioActual;
        _reloj = reloj;
    }

    /// <param name="archivo">La foto tal como llegó del celular. Quien llama la cierra.</param>
    /// <param name="version">La versión que tenía el panel (ver PublicacionDetalle.Version).</param>
    public async Task<FotoSubida> EjecutarAsync(
        Guid publicacionId,
        Stream archivo,
        string version,
        CancellationToken cancellationToken = default)
    {
        var publicacion = await _repositorio.ObtenerAsync(publicacionId, cancellationToken)
            ?? throw new NoEncontradoException("La publicación no existe.");

        PermisosPublicaciones.ExigirEditar(_usuarioActual, publicacion);

        // Procesar antes de guardar nada: si la foto no sirve (400), no queda ningún rastro.
        var procesada = await _procesador.ProcesarAsync(archivo, cancellationToken);

        // Si ya tiene el máximo (422), tampoco se guardó nada todavía.
        var maxFotos = await _referencias.MaxFotosPorPublicacionAsync(cancellationToken);
        var foto = publicacion.AgregarFoto(
            procesada.Detalle.Ancho, procesada.Detalle.Alto, maxFotos, _reloj.GetUtcNow());

        try
        {
            // Primero los archivos y después la base: la base nunca apunta a archivos que no existen.
            await GuardarArchivosAsync(publicacion.EstudioId, foto.Id, procesada, cancellationToken);

            var versionNueva = await _repositorio.GuardarCambiosAsync(publicacion, version, cancellationToken);

            return new FotoSubida(new FotoDePublicacion(foto.Id, foto.Orden, foto.Ancho, foto.Alto), versionNueva);
        }
        catch
        {
            // No se pudo guardar (por ejemplo, 409): sin esto, los archivos quedarían huérfanos.
            await BorrarArchivosAsync(publicacion.EstudioId, foto.Id);
            throw;
        }
    }

    private async Task GuardarArchivosAsync(
        Guid estudioId,
        Guid fotoId,
        FotoProcesada procesada,
        CancellationToken cancellationToken)
    {
        foreach (var versionFoto in procesada.Versiones)
        {
            using var contenido = new MemoryStream(versionFoto.Contenido, writable: false);
            await _almacenamiento.GuardarAsync(
                ClavesFotos.De(estudioId, fotoId, versionFoto.Tamanio), contenido, cancellationToken);
        }
    }

    /// <summary>
    /// Sin CancellationToken a propósito: la limpieza tiene que terminar aunque se haya cancelado el pedido.
    /// </summary>
    private async Task BorrarArchivosAsync(Guid estudioId, Guid fotoId)
    {
        foreach (var clave in ClavesFotos.Todas(estudioId, fotoId))
            await _almacenamiento.BorrarAsync(clave, CancellationToken.None);
    }
}
