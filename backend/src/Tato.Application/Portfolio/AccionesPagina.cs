using Tato.Application.Comun;
using Tato.Application.Estudios;
using Tato.Domain.Comun;
using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Casos de uso que cambian una página: sus datos, su dirección y si se ve en el sitio. Todos cargan la página,
/// verifican que la persona la gestione (<see cref="PermisosPaginas"/>), la cambian y la guardan con la versión que
/// tenía el panel. Cada uno devuelve la versión nueva.
/// </summary>
public sealed class AccionesPagina
{
    private readonly IRepositorioPaginas _repositorio;
    private readonly IUsuarioActual _usuarioActual;
    private readonly TimeProvider _reloj;

    public AccionesPagina(IRepositorioPaginas repositorio, IUsuarioActual usuarioActual, TimeProvider reloj)
    {
        _repositorio = repositorio;
        _usuarioActual = usuarioActual;
        _reloj = reloj;
    }

    /// <summary>
    /// Reemplaza los datos. La dirección no cambia, aunque cambie el nombre: los links ya compartidos siguen
    /// andando (RN-BLG-07).
    /// </summary>
    public async Task<string> EditarAsync(
        Guid id,
        DatosPagina datos,
        string version,
        CancellationToken cancellationToken = default)
    {
        var pagina = await CargarAsync(id, cancellationToken);

        pagina.Editar(datos, _reloj.GetUtcNow());

        return await _repositorio.GuardarCambiosAsync(pagina, version, cancellationToken);
    }

    /// <summary>
    /// Cambia la dirección a propósito. Tiene que estar libre en toda la plataforma (RN-BLG-07).
    /// </summary>
    public async Task<string> CambiarDireccionAsync(
        Guid id,
        string direccion,
        string version,
        CancellationToken cancellationToken = default)
    {
        var pagina = await CargarAsync(id, cancellationToken);
        var nueva = DireccionPublica.Crear(direccion);

        // Volver a guardar la que ya tiene no es usar la de otra página.
        if (nueva != pagina.Direccion && await _repositorio.DireccionEnUsoAsync(nueva, cancellationToken))
            throw new ReglaDeNegocioException(
                "RN-BLG-07", $"La dirección «{nueva}» ya la usa otra página. Elegí otra.");

        pagina.CambiarDireccion(nueva, _reloj.GetUtcNow());

        return await _repositorio.GuardarCambiosAsync(pagina, version, cancellationToken);
    }

    /// <summary>
    /// La muestra en el sitio público.
    /// </summary>
    public async Task<string> MostrarAsync(Guid id, string version, CancellationToken cancellationToken = default)
    {
        var pagina = await CargarAsync(id, cancellationToken);

        pagina.Mostrar(_reloj.GetUtcNow());

        return await _repositorio.GuardarCambiosAsync(pagina, version, cancellationToken);
    }

    /// <summary>
    /// La saca del sitio público sin borrar nada.
    /// </summary>
    public async Task<string> OcultarAsync(Guid id, string version, CancellationToken cancellationToken = default)
    {
        var pagina = await CargarAsync(id, cancellationToken);

        pagina.Ocultar(_reloj.GetUtcNow());

        return await _repositorio.GuardarCambiosAsync(pagina, version, cancellationToken);
    }

    private async Task<PaginaPublica> CargarAsync(Guid id, CancellationToken cancellationToken)
    {
        var pagina = await _repositorio.ObtenerAsync(id, cancellationToken)
            ?? throw new NoEncontradoException("La página no existe.");

        PermisosPaginas.ExigirGestionar(_usuarioActual, pagina.TatuadorId);

        return pagina;
    }
}
