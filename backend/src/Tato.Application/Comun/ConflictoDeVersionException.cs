namespace Tato.Application.Comun;

/// <summary>
/// Alguien modificó el dato después de que se cargó para editar (concurrencia optimista, doc 04). El cambio
/// se rechaza en lugar de pisar el del otro: el panel tiene que recargar y volver a intentar.
/// </summary>
public sealed class ConflictoDeVersionException : Exception
{
    public ConflictoDeVersionException(string mensaje)
        : base(mensaje)
    {
    }
}
