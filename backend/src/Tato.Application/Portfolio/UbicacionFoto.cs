namespace Tato.Application.Portfolio;

/// <summary>
/// Dónde están los archivos de una foto que se puede mostrar en el sitio: el estudio y si es de una página o de una
/// publicación (R-16). Es para armar la clave del almacenamiento; el sitio nunca la recibe.
/// </summary>
public sealed record UbicacionFoto(Guid EstudioId, bool EsDePagina);
