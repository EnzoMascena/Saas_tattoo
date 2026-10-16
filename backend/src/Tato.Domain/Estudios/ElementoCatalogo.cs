using Tato.Domain.Comun;

namespace Tato.Domain.Estudios;

/// <summary>
/// Valor de un catálogo propio de cada estudio, como los estilos o las zonas del cuerpo (R-11).
/// Cada estudio arranca con los valores de <see cref="CatalogosIniciales"/> y después los adapta.
/// <para>
/// Los valores no se borran: se desactivan. Así los trabajos que ya los usan siguen mostrándolos, y no se
/// pueden elegir para trabajos nuevos.
/// </para>
/// </summary>
public abstract class ElementoCatalogo : IPerteneceAEstudio
{
    public const int MaxNombre = 50;

    protected ElementoCatalogo(Guid id, Guid estudioId, string nombre, bool activo)
    {
        Id = id;
        EstudioId = estudioId;
        Nombre = nombre;
        Activo = activo;
    }

    /// <summary>
    /// Identificador único del valor (UUID). Lo genera el dominio al crearlo.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Estudio dueño del catálogo (D-42).
    /// </summary>
    public Guid EstudioId { get; }

    /// <summary>
    /// Nombre que se muestra, por ejemplo «Realismo» o «Antebrazo». Único dentro del catálogo del estudio:
    /// lo controlan el caso de uso y un índice único de la base.
    /// </summary>
    public string Nombre { get; private set; }

    /// <summary>
    /// Si se puede elegir en trabajos nuevos.
    /// </summary>
    public bool Activo { get; private set; }

    public void Renombrar(string nombre)
    {
        Nombre = ValidarNombre(nombre);
    }

    /// <summary>
    /// Deja de ofrecerse para trabajos nuevos. Los que ya lo usan lo conservan.
    /// </summary>
    public void Desactivar()
    {
        Activo = false;
    }

    public void Activar()
    {
        Activo = true;
    }

    protected static Guid ValidarEstudio(Guid estudioId)
    {
        if (estudioId == Guid.Empty)
            throw new ArgumentException("El EstudioId no puede estar vacío.", nameof(estudioId));

        return estudioId;
    }

    protected static string ValidarNombre(string nombre) =>
        Texto.Obligatorio(nombre, MaxNombre, "El nombre", nameof(nombre));
}
