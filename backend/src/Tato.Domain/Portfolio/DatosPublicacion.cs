namespace Tato.Domain.Portfolio;

/// <summary>
/// Datos que se cargan al crear o editar una publicación. Los agrupa para no tener métodos con ocho parámetros.
/// <para>
/// El estilo, la zona y el tatuador son del módulo Estudios: acá se guardan solo sus Ids. Que existan, que sean
/// del estudio y que estén activos lo controla el caso de uso, que es el que puede consultarlos.
/// </para>
/// </summary>
/// <param name="Titulo">Título de la publicación. Obligatorio.</param>
/// <param name="Descripcion">Texto libre, de hasta 2200 caracteres (RN-BLG-11). Nunca lleva datos del cliente (RN-BLG-02).</param>
/// <param name="FechaTrabajo">Día en que se hizo el tatuaje, sin hora.</param>
/// <param name="EstiloId">Estilo del catálogo del estudio (R-11).</param>
/// <param name="ZonaCuerpoId">Zona del cuerpo del catálogo del estudio (R-11).</param>
/// <param name="TatuadorId">Artista de cuya página es la publicación. Sin artista, es de la página del estudio (RN-BLG-12).</param>
/// <param name="Destacada">Si aparece fijada al principio de su página (RN-BLG-13).</param>
/// <param name="Tipo">Si es un trabajo o una publicación del blog (D-50). Por defecto, Trabajo.</param>
public sealed record DatosPublicacion(
    string Titulo,
    string? Descripcion = null,
    DateOnly? FechaTrabajo = null,
    Guid? EstiloId = null,
    Guid? ZonaCuerpoId = null,
    Guid? TatuadorId = null,
    bool Destacada = false,
    TipoPublicacion Tipo = TipoPublicacion.Trabajo);
