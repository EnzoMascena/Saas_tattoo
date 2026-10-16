namespace Tato.Application.Comun;

/// <summary>
/// Una página de resultados, con el total de la lista completa para armar la paginación en el panel.
/// </summary>
public sealed record Pagina<T>(IReadOnlyList<T> Items, int NumeroPagina, int TamanioPagina, int Total)
{
    public int TotalPaginas => (Total + TamanioPagina - 1) / TamanioPagina;
}
