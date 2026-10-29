using Tato.Domain.Portfolio;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// Atajos para armar lo que ve un visitante del sitio, con los mismos casos de uso del panel: páginas visibles y
/// publicaciones publicadas.
/// </summary>
internal sealed partial class Panel
{
    /// <summary>
    /// Crea la página del estudio o la propia de artista y la muestra. Devuelve su Id y su dirección.
    /// </summary>
    public async Task<(Guid Id, DireccionPublica Direccion)> CrearPaginaVisibleAsync(bool delEstudio, string nombre)
    {
        var id = await CrearPaginaAsync(delEstudio, new DatosPagina(nombre));
        var pagina = await ObtenerPaginaAsync(id);
        await MostrarPaginaAsync(id, pagina.Version);

        return (id, DireccionPublica.Crear(pagina.Direccion));
    }

    /// <summary>
    /// Crea una publicación, le sube las fotos, confirma el consentimiento y la publica. Devuelve su Id.
    /// </summary>
    public async Task<Guid> PublicarNuevaAsync(DatosPublicacion datos, int fotos = 1)
    {
        var id = await CrearPublicacionAsync(datos);
        var version = (await ObtenerAsync(id)).Version;

        for (var i = 0; i < fotos; i++)
            version = (await SubirFotoAsync(id, version)).Version;

        version = await ConfirmarConsentimientoAsync(id, version);
        await PublicarAsync(id, version);

        return id;
    }
}
