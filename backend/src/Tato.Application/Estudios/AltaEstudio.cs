using Tato.Domain.Estudios;

namespace Tato.Application.Estudios;

/// <summary>
/// Caso de uso: dar de alta un estudio (tenant). En la versión 1 lo usa el desarrollador desde la consola (P-19);
/// en la 2 lo va a usar el alta por autoservicio.
/// </summary>
public sealed class AltaEstudio
{
    private readonly IRegistroEstudios _registro;

    public AltaEstudio(IRegistroEstudios registro)
    {
        _registro = registro;
    }

    /// <summary>
    /// Crea el estudio con sus catálogos iniciales de estilos y zonas del cuerpo, y devuelve su Id.
    /// Valida el dominio: si un dato no sirve, lanza ArgumentException.
    /// Si la zona horaria viene vacía, se usa la de Argentina (D-43).
    /// </summary>
    public async Task<Guid> EjecutarAsync(
        string nombre,
        string plan,
        string? zonaHoraria,
        CancellationToken cancellationToken = default)
    {
        var estudio = Estudio.Crear(nombre, plan, zonaHoraria);

        // Cada estudio arranca con los catálogos iniciales y después los adapta (R-11).
        var estilos = CatalogosIniciales.CrearEstilos(estudio.Id);
        var zonasCuerpo = CatalogosIniciales.CrearZonasCuerpo(estudio.Id);

        await _registro.AgregarEstudioAsync(estudio, estilos, zonasCuerpo, cancellationToken);

        return estudio.Id;
    }
}