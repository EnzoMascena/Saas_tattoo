namespace Tato.Domain.Portfolio;

/// <summary>
/// Datos que se cargan al crear o editar una publicación. Los agrupa para no tener métodos con siete parámetros.
/// <para>
/// El estilo, la zona y el tatuador son del módulo Estudios: acá se guardan solo sus Ids. Que existan, que sean
/// del estudio y que estén activos lo controla el caso de uso, que es el que puede consultarlos.
/// </para>
/// </summary>
/// <param name="Titulo">Título del trabajo. Obligatorio.</param>
/// <param name="Descripcion">Texto libre. Nunca lleva datos del cliente (RN-BLG-02).</param>
/// <param name="FechaTrabajo">Día en que se hizo el tatuaje, sin hora.</param>
/// <param name="EstiloId">Estilo del catálogo del estudio (R-11).</param>
/// <param name="ZonaCuerpoId">Zona del cuerpo del catálogo del estudio (R-11).</param>
/// <param name="TatuadorId">Perfil del tatuador que lo hizo.</param>
/// <param name="Destacada">Si aparece entre los destacados del inicio del sitio.</param>
public sealed record DatosPublicacion(
    string Titulo,
    string? Descripcion = null,
    DateOnly? FechaTrabajo = null,
    Guid? EstiloId = null,
    Guid? ZonaCuerpoId = null,
    Guid? TatuadorId = null,
    bool Destacada = false);
