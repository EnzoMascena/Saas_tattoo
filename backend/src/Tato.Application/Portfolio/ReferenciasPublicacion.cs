using Tato.Application.Estudios;
using Tato.Domain.Comun;
using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Controla que el estilo, la zona y el tatuador de una publicación se puedan usar. Lo comparten crear y editar.
/// <para>
/// Solo se controlan los Ids que cambian respecto de <paramref name="actual"/>: un trabajo que ya usa un estilo
/// desactivado lo conserva al editarse (R-11). Al crear no hay publicación actual y se controlan todos.
/// </para>
/// </summary>
internal static class ReferenciasPublicacion
{
    public static async Task VerificarAsync(
        DatosPublicacion datos,
        Publicacion? actual,
        IReferenciasDelEstudio referencias,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(datos);

        if (datos.EstiloId is { } estiloId
            && estiloId != actual?.EstiloId
            && !await referencias.EstiloDisponibleAsync(estiloId, cancellationToken))
            throw new ReglaDeNegocioException("R-11", "El estilo elegido no existe o está desactivado.");

        if (datos.ZonaCuerpoId is { } zonaCuerpoId
            && zonaCuerpoId != actual?.ZonaCuerpoId
            && !await referencias.ZonaCuerpoDisponibleAsync(zonaCuerpoId, cancellationToken))
            throw new ReglaDeNegocioException("R-11", "La zona del cuerpo elegida no existe o está desactivada.");

        if (datos.TatuadorId is { } tatuadorId
            && tatuadorId != actual?.TatuadorId
            && !await referencias.TatuadorExisteAsync(tatuadorId, cancellationToken))
            throw new ArgumentException("El tatuador elegido no existe.", nameof(datos.TatuadorId));
    }
}
