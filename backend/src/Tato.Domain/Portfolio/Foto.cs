using Tato.Domain.Comun;

namespace Tato.Domain.Portfolio;

/// <summary>
/// Foto de una publicación (R-04). Solo se crea, se ordena y se quita a través de su <see cref="Publicacion"/>,
/// para que las reglas de cantidad y orden no se puedan saltear.
/// <para>
/// No guarda la ruta de los archivos: el almacenamiento los ubica por el Id de la foto. Guarda el ancho y el
/// alto de la versión optimizada para que el sitio reserve el espacio antes de que cargue la imagen.
/// </para>
/// </summary>
public sealed class Foto : IPerteneceAEstudio
{
    private Foto(Guid id, Guid estudioId, Guid publicacionId, int orden, int ancho, int alto, DateTimeOffset subidaEn)
    {
        Id = id;
        EstudioId = estudioId;
        PublicacionId = publicacionId;
        Orden = orden;
        Ancho = ancho;
        Alto = alto;
        SubidaEn = subidaEn;
    }

    /// <summary>
    /// Identificador único de la foto (UUID). También identifica sus archivos en el almacenamiento.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Estudio de la foto (D-42). Se copia de la publicación.
    /// </summary>
    public Guid EstudioId { get; }

    public Guid PublicacionId { get; }

    /// <summary>
    /// Posición dentro de la publicación, desde 0 y sin huecos. La 0 es la portada.
    /// </summary>
    public int Orden { get; private set; }

    /// <summary>
    /// Ancho en píxeles de la versión optimizada.
    /// </summary>
    public int Ancho { get; }

    /// <summary>
    /// Alto en píxeles de la versión optimizada.
    /// </summary>
    public int Alto { get; }

    /// <summary>
    /// Cuándo se subió, en UTC (D-43).
    /// </summary>
    public DateTimeOffset SubidaEn { get; }

    internal static Foto Crear(Publicacion publicacion, int orden, int ancho, int alto, DateTimeOffset subidaEn)
    {
        if (ancho <= 0)
            throw new ArgumentOutOfRangeException(nameof(ancho), ancho, "El ancho tiene que ser mayor a 0.");

        if (alto <= 0)
            throw new ArgumentOutOfRangeException(nameof(alto), alto, "El alto tiene que ser mayor a 0.");

        return new Foto(Guid.NewGuid(), publicacion.EstudioId, publicacion.Id, orden, ancho, alto, subidaEn);
    }

    internal void CambiarOrden(int orden)
    {
        Orden = orden;
    }
}
