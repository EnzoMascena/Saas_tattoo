namespace Tato.Application.Comun;

/// <summary>
/// Qué página de una lista se pide. Corrige lo que manda el cliente: la página arranca en 1 y el tamaño queda entre
/// 1 y <see cref="TamanioMaximo"/>, así nadie puede pedir un millón de filas de una vez.
/// </summary>
public sealed record Paginacion
{
    public const int TamanioPorDefecto = 20;
    public const int TamanioMaximo = 50;

    public Paginacion(int numeroPagina = 1, int tamanioPagina = TamanioPorDefecto)
    {
        NumeroPagina = Math.Max(1, numeroPagina);
        TamanioPagina = Math.Clamp(tamanioPagina, 1, TamanioMaximo);
    }

    public int NumeroPagina { get; }

    public int TamanioPagina { get; }

    /// <summary>
    /// Cuántas filas saltear para llegar a esta página. Se calcula en long para que una página enorme no desborde.
    /// </summary>
    public int Saltear => (int)Math.Min((long)(NumeroPagina - 1) * TamanioPagina, int.MaxValue);
}
