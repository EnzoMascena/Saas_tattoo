using Tato.Application.Comun;
using Tato.Application.Estudios;

namespace Tato.Application.Portfolio;

/// <summary>
/// Caso de uso: abrir una versión de una foto para el panel. No consulta la base: la clave del archivo lleva el
/// estudio de la sesión, así que una foto de otro estudio da una ruta que no existe (D-42).
/// </summary>
public sealed class LeerFoto
{
    private readonly IEstudioActual _estudioActual;
    private readonly IAlmacenamientoArchivos _almacenamiento;

    public LeerFoto(IEstudioActual estudioActual, IAlmacenamientoArchivos almacenamiento)
    {
        _estudioActual = estudioActual;
        _almacenamiento = almacenamiento;
    }

    /// <summary>
    /// Abre el archivo WebP, o devuelve null si no existe. Quien lo abre lo cierra.
    /// </summary>
    public Task<Stream?> EjecutarAsync(
        Guid fotoId,
        TamanioFoto tamanio,
        CancellationToken cancellationToken = default)
    {
        var estudioId = _estudioActual.EstudioId
            ?? throw new InvalidOperationException("No hay un estudio en la sesión.");

        return _almacenamiento.AbrirAsync(ClavesFotos.De(estudioId, fotoId, tamanio), cancellationToken);
    }
}
