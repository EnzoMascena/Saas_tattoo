using Tato.Application.Comun;

namespace Tato.Application.Portfolio;

/// <summary>
/// Caso de uso: abrir una versión de una foto para el sitio público, sin sesión. A diferencia de <see cref="LeerFoto"/>,
/// que arma la clave con el estudio de la sesión, acá primero se pregunta a la base si la foto se puede mostrar y de
/// qué estudio es: así solo salen las fotos de lo que está publicado y visible.
/// </summary>
public sealed class LeerFotoDelSitio
{
    private readonly IConsultasDelSitio _consultas;
    private readonly IAlmacenamientoArchivos _almacenamiento;

    public LeerFotoDelSitio(IConsultasDelSitio consultas, IAlmacenamientoArchivos almacenamiento)
    {
        _consultas = consultas;
        _almacenamiento = almacenamiento;
    }

    /// <summary>
    /// Abre el archivo WebP, o devuelve null si la foto no existe o no se puede mostrar. Quien lo abre lo cierra.
    /// </summary>
    public async Task<Stream?> EjecutarAsync(
        Guid fotoId,
        TamanioFoto tamanio,
        CancellationToken cancellationToken = default)
    {
        var ubicacion = await _consultas.UbicarFotoAsync(fotoId, cancellationToken);

        if (ubicacion is null)
            return null;

        var clave = ubicacion.EsDePagina
            ? ClavesFotos.DePagina(ubicacion.EstudioId, fotoId, tamanio)
            : ClavesFotos.De(ubicacion.EstudioId, fotoId, tamanio);

        return await _almacenamiento.AbrirAsync(clave, cancellationToken);
    }
}
