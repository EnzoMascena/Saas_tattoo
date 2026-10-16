using Tato.Application.Comun;
using Tato.Application.Estudios;
using Tato.Domain.Comun;
using Tato.Domain.Portfolio;

namespace Tato.Application.Portfolio;

/// <summary>
/// Caso de uso: crear la página del estudio o la propia de artista. Nace oculta y su dirección sale del nombre
/// (RN-BLG-07). Hay una sola por estudio y una sola por artista (D-49).
/// </summary>
public sealed class CrearPagina
{
    private readonly IRepositorioPaginas _repositorio;
    private readonly IEstudioActual _estudioActual;
    private readonly IUsuarioActual _usuarioActual;
    private readonly TimeProvider _reloj;

    public CrearPagina(
        IRepositorioPaginas repositorio,
        IEstudioActual estudioActual,
        IUsuarioActual usuarioActual,
        TimeProvider reloj)
    {
        _repositorio = repositorio;
        _estudioActual = estudioActual;
        _usuarioActual = usuarioActual;
        _reloj = reloj;
    }

    /// <summary>
    /// Crea la página y devuelve su Id.
    /// </summary>
    /// <param name="delEstudio">true para la del estudio; false para la del artista de la sesión.</param>
    public async Task<Guid> EjecutarAsync(
        bool delEstudio,
        DatosPagina datos,
        CancellationToken cancellationToken = default)
    {
        var estudioId = _estudioActual.EstudioId
            ?? throw new InvalidOperationException("No hay un estudio en la sesión.");

        // La página de artista es siempre la del perfil de la sesión, nunca la de un Id que mande el navegador
        // (D-51). Sin perfil de tatuador no hay página de artista.
        var tatuadorId = delEstudio ? null : _usuarioActual.TatuadorId;

        if (!delEstudio && tatuadorId is null)
            throw new AccesoDenegadoException("Para tener una página de artista hace falta un perfil de tatuador.");

        PermisosPaginas.ExigirGestionar(_usuarioActual, tatuadorId);

        // El dominio valida los datos y arma la dirección con el nombre, sin ir a la base.
        var ahora = _reloj.GetUtcNow();
        var pagina = tatuadorId is { } id
            ? PaginaPublica.CrearDeArtista(estudioId, id, datos, ahora)
            : PaginaPublica.CrearDelEstudio(estudioId, datos, ahora);

        if (await _repositorio.ExisteAsync(tatuadorId, cancellationToken))
            throw new ReglaDeNegocioException(
                "D-49", delEstudio ? "El estudio ya tiene su página." : "Ya tenés tu página de artista.");

        if (await _repositorio.DireccionEnUsoAsync(pagina.Direccion, cancellationToken))
            throw new ReglaDeNegocioException(
                "RN-BLG-07",
                $"La dirección «{pagina.Direccion}» ya la usa otra página. Como sale del nombre, probá con otro.");

        await _repositorio.AgregarAsync(pagina, cancellationToken);

        return pagina.Id;
    }
}
