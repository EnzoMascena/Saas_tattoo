namespace Tato.Domain.Estudios;

/// <summary>
/// Valores con los que arranca el catálogo de cada estudio nuevo (R-11). Son un punto de partida, como los
/// parámetros por defecto de <see cref="Estudio"/> (D-34): después cada estudio agrega, renombra o desactiva.
/// </summary>
public static class CatalogosIniciales
{
    public static IReadOnlyList<string> Estilos { get; } =
    [
        "Realismo",
        "Black and grey",
        "Blackwork",
        "Fineline",
        "Tradicional",
        "Neotradicional",
        "Japonés",
        "Lettering",
        "Acuarela",
        "Geométrico",
        "Minimalista",
        "Dotwork",
        "Ornamental",
    ];

    public static IReadOnlyList<string> ZonasCuerpo { get; } =
    [
        "Brazo",
        "Antebrazo",
        "Mano",
        "Hombro",
        "Pecho",
        "Espalda",
        "Abdomen",
        "Costillas",
        "Cadera",
        "Muslo",
        "Pierna",
        "Pantorrilla",
        "Tobillo",
        "Pie",
        "Cuello",
        "Nuca",
    ];

    /// <summary>
    /// Crea los estilos iniciales de un estudio.
    /// </summary>
    public static IReadOnlyList<Estilo> CrearEstilos(Guid estudioId) =>
        [.. Estilos.Select(nombre => Estilo.Crear(estudioId, nombre))];

    /// <summary>
    /// Crea las zonas del cuerpo iniciales de un estudio.
    /// </summary>
    public static IReadOnlyList<ZonaCuerpo> CrearZonasCuerpo(Guid estudioId) =>
        [.. ZonasCuerpo.Select(nombre => ZonaCuerpo.Crear(estudioId, nombre))];
}
