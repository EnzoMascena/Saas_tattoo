namespace Tato.Domain.Estudios;

/// <summary>
/// Estilo de tatuaje del catálogo del estudio (R-11): Realismo, Fineline, Blackwork, etc.
/// Lo usan las publicaciones del blog y, más adelante, los tatuajes del MVP.
/// </summary>
public sealed class Estilo : ElementoCatalogo
{
    private Estilo(Guid id, Guid estudioId, string nombre, bool activo)
        : base(id, estudioId, nombre, activo)
    {
    }

    /// <summary>
    /// Crea un estilo activo para el estudio.
    /// </summary>
    public static Estilo Crear(Guid estudioId, string nombre) =>
        new(Guid.NewGuid(), ValidarEstudio(estudioId), ValidarNombre(nombre), activo: true);
}
