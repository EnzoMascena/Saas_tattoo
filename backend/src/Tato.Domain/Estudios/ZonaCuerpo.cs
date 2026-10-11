namespace Tato.Domain.Estudios;

/// <summary>
/// Zona del cuerpo del catálogo del estudio (R-11): Antebrazo, Espalda, Costillas, etc.
/// Lo usan las publicaciones del blog y, más adelante, los tatuajes del MVP.
/// </summary>
public sealed class ZonaCuerpo : ElementoCatalogo
{
    private ZonaCuerpo(Guid id, Guid estudioId, string nombre, bool activo)
        : base(id, estudioId, nombre, activo)
    {
    }

    /// <summary>
    /// Crea una zona activa para el estudio.
    /// </summary>
    public static ZonaCuerpo Crear(Guid estudioId, string nombre) =>
        new(Guid.NewGuid(), ValidarEstudio(estudioId), ValidarNombre(nombre), activo: true);
}
