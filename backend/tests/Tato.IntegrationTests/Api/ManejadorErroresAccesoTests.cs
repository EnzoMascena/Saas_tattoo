using Tato.Api.Comun;
using Tato.Application.Comun;

namespace Tato.IntegrationTests.Api;

/// <summary>
/// Respuestas ante datos que no existen, permisos y ediciones simultáneas.
/// </summary>
public sealed class ManejadorErroresAccesoTests
{
    [Fact]
    public void Traducir_NoEncontrado_Devuelve404ConElMensaje()
    {
        var problema = ManejadorErrores.Traducir(new NoEncontradoException("La publicación no existe."));

        Assert.Equal(404, problema.Status);
        Assert.Equal("La publicación no existe.", problema.Detail);
    }

    [Fact]
    public void Traducir_AccesoDenegado_Devuelve403ConElMensaje()
    {
        var problema = ManejadorErrores.Traducir(
            new AccesoDenegadoException("Solo un Dueño o un Administrativo puede publicar."));

        Assert.Equal(403, problema.Status);
        Assert.Equal("Solo un Dueño o un Administrativo puede publicar.", problema.Detail);
    }

    [Fact]
    public void Traducir_ConflictoDeVersion_Devuelve409ConElMensaje()
    {
        var problema = ManejadorErrores.Traducir(
            new ConflictoDeVersionException("Recargá la publicación para ver los cambios."));

        Assert.Equal(409, problema.Status);
        Assert.Equal("Recargá la publicación para ver los cambios.", problema.Detail);
    }
}
