namespace Tato.Application.Estudios;

/// <summary>
/// Lo que otros módulos pueden preguntarle a Estudios sobre sus datos, sin usar sus clases (D-38).
/// Todas las respuestas son del estudio de la sesión (D-42): un Id de otro estudio da false, igual que uno
/// que no existe.
/// </summary>
public interface IReferenciasDelEstudio
{
    /// <summary>
    /// Si el estilo existe y está activo, es decir si se puede elegir para un trabajo nuevo (R-11).
    /// </summary>
    Task<bool> EstiloDisponibleAsync(Guid estiloId, CancellationToken cancellationToken);

    /// <summary>
    /// Si la zona del cuerpo existe y está activa (R-11).
    /// </summary>
    Task<bool> ZonaCuerpoDisponibleAsync(Guid zonaCuerpoId, CancellationToken cancellationToken);

    /// <summary>
    /// Si el perfil de tatuador existe.
    /// </summary>
    Task<bool> TatuadorExisteAsync(Guid tatuadorId, CancellationToken cancellationToken);
}
