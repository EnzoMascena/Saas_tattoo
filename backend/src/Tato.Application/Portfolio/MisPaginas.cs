using Tato.Application.Estudios;

namespace Tato.Application.Portfolio;

/// <summary>
/// Caso de uso: las páginas que gestiona la persona de la sesión, para la pantalla «Mi página». Primero la suya de
/// artista y después, si es Dueño, la del estudio. Las que todavía no se crearon no aparecen.
/// </summary>
public sealed class MisPaginas
{
    private readonly IConsultasPaginas _consultas;
    private readonly IUsuarioActual _usuarioActual;

    public MisPaginas(IConsultasPaginas consultas, IUsuarioActual usuarioActual)
    {
        _consultas = consultas;
        _usuarioActual = usuarioActual;
    }

    public async Task<IReadOnlyList<PaginaDetalle>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var paginas = await _consultas.ListarAsync(cancellationToken);

        return
        [
            .. paginas
                .Where(p => PermisosPaginas.PuedeGestionar(_usuarioActual, p.TatuadorId))
                .OrderBy(p => p.EsDelEstudio),
        ];
    }
}
