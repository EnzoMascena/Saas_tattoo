namespace Tato.Domain.Estudios;

/// <summary>
/// Un estudio de tatuajes: tenant raíz. Sus datos están completamente aislados de otros estudios (RN-VIS-01).
/// </summary>
public sealed class Estudio
{
    public const int MaxNombre = 100;
    public const int MaxPlan = 50;
    public const int MaxZonaHoraria = 64;

    public const string ZonaHorariaPorDefecto = "America/Argentina/Buenos_Aires";
    public const int DiasVencimientoSeniaPorDefecto = 14;
    public const int MaxReagendamientosPorDefecto = 2;
    public const int MaxFotosPorPublicacionPorDefecto = 10;

    private Estudio(
        Guid id,
        string nombre,
        string plan,
        string zonaHoraria,
        int diasVencimientoSenia,
        int maxReagendamientos,
        int maxFotosPorPublicacion)
    {
        Id = id;
        Nombre = nombre;
        Plan = plan;
        ZonaHoraria = zonaHoraria;
        DiasVencimientoSenia = diasVencimientoSenia;
        MaxReagendamientos = maxReagendamientos;
        MaxFotosPorPublicacion = maxFotosPorPublicacion;
    }

    /// <summary>
    /// Identificador único del estudio (UUID). Lo genera el dominio al crear el estudio.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Nombre del estudio.
    /// </summary>
    public string Nombre { get; private set; }

    /// <summary>
    /// Plan de facturación (todavía no definido).
    /// </summary>
    public string Plan { get; private set; }

    /// <summary>
    /// Zona horaria del estudio (ej: "America/Argentina/Buenos_Aires").
    /// Las reglas como "00:00 del día siguiente" (RN-TUR-06) se aplican en esta zona.
    /// Por defecto, Argentina (D-43).
    /// </summary>
    public string ZonaHoraria { get; private set; }

    /// <summary>
    /// Días que dura una seña desde el pago (por defecto, 14). Configurable por estudio (D-34).
    /// </summary>
    public int DiasVencimientoSenia { get; private set; }

    /// <summary>
    /// Máximo de reagendamientos permitidos (por defecto, 2). Configurable por estudio (D-34).
    /// </summary>
    public int MaxReagendamientos { get; private set; }

    /// <summary>
    /// Máximo de fotos por publicación del blog (por defecto, 10). Configurable por estudio (RN-BLG-04, D-34).
    /// </summary>
    public int MaxFotosPorPublicacion { get; private set; }

    public static Estudio Crear(
        string nombre,
        string plan,
        string? zonaHoraria = null,
        int? diasVencimientoSenia = null,
        int? maxReagendamientos = null,
        int? maxFotosPorPublicacion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del estudio no puede estar vacio.", nameof(nombre));

        var nombreLimpio = nombre.Trim();

        if (nombreLimpio.Length > MaxNombre)
            throw new ArgumentException($"El nombre no puede exceder {MaxNombre} caracteres.", nameof(nombre));

        if (string.IsNullOrWhiteSpace(plan))
            throw new ArgumentException("El plan no puede estar vacio.", nameof(plan));

        var planLimpio = plan.Trim();

        if (planLimpio.Length > MaxPlan)
            throw new ArgumentException($"El plan no puede exceder {MaxPlan} caracteres.", nameof(plan));

        // Si no se especifica, se usa la zona por defecto (D-43).
        var zona = string.IsNullOrWhiteSpace(zonaHoraria) ? ZonaHorariaPorDefecto : zonaHoraria.Trim();

        if (zona.Length > MaxZonaHoraria)
            throw new ArgumentException(
                $"La zona horaria no puede exceder {MaxZonaHoraria} caracteres.", nameof(zonaHoraria));

        zona = ValidarZonaHoraria(zona);

        // Si no se especifican, se usan los valores por defecto (D-34).
        var dias = diasVencimientoSenia ?? DiasVencimientoSeniaPorDefecto;
        var max = maxReagendamientos ?? MaxReagendamientosPorDefecto;
        var maxFotos = maxFotosPorPublicacion ?? MaxFotosPorPublicacionPorDefecto;

        ValidarParametros(dias, max);
        ValidarMaxFotos(maxFotos);

        return new Estudio(Guid.NewGuid(), nombreLimpio, planLimpio, zona, dias, max, maxFotos);
    }

    /// <summary>
    /// Cambia los parámetros de negocio del estudio (plazo de seña, máx. reagendamientos).
    /// Solo el Dueño puede hacerlo.
    /// </summary>
    public void ConfigurarParametros(int diasVencimientoSenia, int maxReagendamientos)
    {
        ValidarParametros(diasVencimientoSenia, maxReagendamientos);

        DiasVencimientoSenia = diasVencimientoSenia;
        MaxReagendamientos = maxReagendamientos;
    }

    /// <summary>
    /// Cambia el máximo de fotos por publicación. Solo el Dueño puede hacerlo.
    /// </summary>
    public void ConfigurarMaxFotosPorPublicacion(int maxFotosPorPublicacion)
    {
        ValidarMaxFotos(maxFotosPorPublicacion);

        MaxFotosPorPublicacion = maxFotosPorPublicacion;
    }

    /// <summary>
    /// Valida los parámetros de negocio del estudio (D-34).
    /// </summary>
    private static void ValidarParametros(int diasVencimientoSenia, int maxReagendamientos)
    {
        if (diasVencimientoSenia <= 0)
            throw new ArgumentException(
                "Los días de vencimiento de la seña deben ser mayor a 0.", nameof(diasVencimientoSenia));

        if (maxReagendamientos < 0)
            throw new ArgumentException(
                "El máximo de reagendamientos no puede ser negativo.", nameof(maxReagendamientos));
    }

    /// <summary>
    /// Valida que la zona horaria exista y sea un identificador IANA (D-43), y la devuelve con su nombre
    /// oficial: «america/argentina/buenos_aires» se guarda como «America/Argentina/Buenos_Aires».
    /// Rechaza los identificadores de Windows («Argentina Standard Time») y los inventados («Argentina»).
    /// </summary>
    private static string ValidarZonaHoraria(string zonaHoraria)
    {
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(zonaHoraria, out var zona) || !zona.HasIanaId)
            throw new ArgumentException(
                $"La zona horaria «{zonaHoraria}» no existe. Usá un identificador IANA, " +
                $"como {ZonaHorariaPorDefecto}.",
                nameof(zonaHoraria));

        return zona.Id;
    }

    /// <summary>
    /// Valida el máximo de fotos por publicación (RN-BLG-04).
    /// </summary>
    private static void ValidarMaxFotos(int maxFotosPorPublicacion)
    {
        if (maxFotosPorPublicacion <= 0)
            throw new ArgumentException(
                "El máximo de fotos por publicación tiene que ser mayor a 0.", nameof(maxFotosPorPublicacion));
    }
}
