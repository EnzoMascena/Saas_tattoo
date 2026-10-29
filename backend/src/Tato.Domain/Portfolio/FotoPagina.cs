namespace Tato.Domain.Portfolio;

/// <summary>
/// Una foto de una página pública: la foto o el logo del encabezado, o la de «Sobre» (R-16). Es un objeto de valor:
/// reemplazarla es subir otra, con otro Id.
/// <para>
/// Como en <see cref="Foto"/>, no guarda la ruta de los archivos: el almacenamiento los ubica por el Id. El ancho y
/// el alto son los de la versión de detalle, para que el sitio reserve el espacio antes de que cargue la imagen.
/// </para>
/// </summary>
public sealed record FotoPagina
{
    private FotoPagina(Guid id, int ancho, int alto)
    {
        Id = id;
        Ancho = ancho;
        Alto = alto;
    }

    public Guid Id { get; }

    public int Ancho { get; }

    public int Alto { get; }

    /// <summary>
    /// Una foto recién subida, con un Id nuevo y las medidas de su versión de detalle.
    /// </summary>
    public static FotoPagina Crear(int ancho, int alto)
    {
        if (ancho <= 0)
            throw new ArgumentOutOfRangeException(nameof(ancho), ancho, "El ancho tiene que ser mayor a 0.");

        if (alto <= 0)
            throw new ArgumentOutOfRangeException(nameof(alto), alto, "El alto tiene que ser mayor a 0.");

        return new FotoPagina(Guid.NewGuid(), ancho, alto);
    }
}
