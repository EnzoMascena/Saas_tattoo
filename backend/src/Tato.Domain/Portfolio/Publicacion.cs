using Tato.Domain.Comun;
using Tato.Domain.Estudios;

namespace Tato.Domain.Portfolio;

/// <summary>
/// Una publicación de una página pública: un Trabajo o un Blog (D-50, doc 07, sección 6). Es la raíz del
/// agregado: sus fotos se agregan, se quitan y se ordenan solo a través de ella, así las reglas no se pueden
/// saltear.
/// <para>
/// Nunca guarda datos del cliente (RN-BLG-02). Todos los métodos que cambian algo reciben «ahora» en UTC
/// (D-43) y actualizan <see cref="ActualizadaEn"/>: así cualquier cambio, también en las fotos, modifica la
/// fila de la publicación y la concurrencia optimista lo detecta.
/// </para>
/// </summary>
public sealed class Publicacion : IPerteneceAEstudio
{
    public const int MaxTitulo = 120;
    // Hasta 2200 caracteres, el mismo límite que Instagram (RN-BLG-11).
    public const int MaxDescripcion = 2200;

    private readonly List<Foto> _fotos = [];

    private Publicacion(Guid id, Guid estudioId, Guid creadaPor, DateTimeOffset creadaEn)
    {
        Id = id;
        EstudioId = estudioId;
        CreadaPor = creadaPor;
        CreadaEn = creadaEn;
        ActualizadaEn = creadaEn;
        Estado = EstadoPublicacion.Borrador;
        Titulo = string.Empty;
    }

    /// <summary>
    /// Identificador único de la publicación (UUID). Lo genera el dominio al crearla.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Estudio al que pertenece (D-42).
    /// </summary>
    public Guid EstudioId { get; }

    public string Titulo { get; private set; }

    /// <summary>
    /// En qué pestaña de la página pública aparece: Trabajos o Blog (D-50).
    /// </summary>
    public TipoPublicacion Tipo { get; private set; }

    /// <summary>
    /// Texto libre de la publicación. Opcional.
    /// </summary>
    public string? Descripcion { get; private set; }

    /// <summary>
    /// Día en que se hizo el tatuaje. Opcional.
    /// </summary>
    public DateOnly? FechaTrabajo { get; private set; }

    /// <summary>
    /// Estilo del catálogo del estudio (R-11). Opcional.
    /// </summary>
    public Guid? EstiloId { get; private set; }

    /// <summary>
    /// Zona del cuerpo del catálogo del estudio (R-11). Opcional.
    /// </summary>
    public Guid? ZonaCuerpoId { get; private set; }

    /// <summary>
    /// Artista de cuya página es la publicación. Sin artista, es de la página del estudio (RN-BLG-12).
    /// </summary>
    public Guid? TatuadorId { get; private set; }

    /// <summary>
    /// Si está fijada: aparece primero en su página (RN-BLG-13).
    /// </summary>
    public bool Destacada { get; private set; }

    public EstadoPublicacion Estado { get; private set; }

    /// <summary>
    /// Usuario que confirmó que el cliente aceptó difundir las fotos (RN-BLG-03). Null si nadie lo confirmó.
    /// </summary>
    public Guid? ConsentimientoConfirmadoPor { get; private set; }

    /// <summary>
    /// Cuándo se confirmó el consentimiento, en UTC.
    /// </summary>
    public DateTimeOffset? ConsentimientoConfirmadoEn { get; private set; }

    public bool TieneConsentimiento => ConsentimientoConfirmadoPor is not null;

    /// <summary>
    /// Usuario que creó la publicación.
    /// </summary>
    public Guid CreadaPor { get; }

    public DateTimeOffset CreadaEn { get; }

    /// <summary>
    /// Último cambio de cualquier tipo: datos, fotos, consentimiento o estado.
    /// </summary>
    public DateTimeOffset ActualizadaEn { get; private set; }

    /// <summary>
    /// La primera vez que se publicó. No cambia si se oculta y se vuelve a publicar: el sitio ordena por esta fecha.
    /// </summary>
    public DateTimeOffset? PublicadaEn { get; private set; }

    /// <summary>
    /// Fotos en el orden en que se muestran.
    /// </summary>
    public IReadOnlyList<Foto> Fotos => [.. _fotos.OrderBy(f => f.Orden)];

    /// <summary>
    /// La portada es siempre la primera foto (RN-BLG-04). No hay un campo «es portada» que se pueda
    /// desincronizar: elegir la portada es mover esa foto al principio.
    /// </summary>
    public Foto? Portada => _fotos.MinBy(f => f.Orden);

    /// <summary>
    /// Crea una publicación en Borrador.
    /// </summary>
    public static Publicacion Crear(Guid estudioId, Guid creadaPor, DatosPublicacion datos, DateTimeOffset ahora)
    {
        if (estudioId == Guid.Empty)
            throw new ArgumentException("El EstudioId no puede estar vacío.", nameof(estudioId));

        if (creadaPor == Guid.Empty)
            throw new ArgumentException("El usuario que la crea no puede estar vacío.", nameof(creadaPor));

        FechaUtc.Validar(ahora, nameof(ahora));

        var publicacion = new Publicacion(Guid.NewGuid(), estudioId, creadaPor, ahora);
        publicacion.AplicarDatos(datos);

        return publicacion;
    }

    /// <summary>
    /// Cambia los datos. Se puede en cualquier estado: corregir un trabajo publicado no obliga a ocultarlo.
    /// </summary>
    public void Editar(DatosPublicacion datos, DateTimeOffset ahora)
    {
        FechaUtc.Validar(ahora, nameof(ahora));

        AplicarDatos(datos);
        ActualizadaEn = ahora;
    }

    /// <summary>
    /// Agrega una foto al final. El máximo lo define cada estudio (RN-BLG-04, D-34).
    /// </summary>
    /// <param name="ancho">Ancho de la versión optimizada, en píxeles.</param>
    /// <param name="alto">Alto de la versión optimizada, en píxeles.</param>
    /// <param name="maxFotos">Máximo de fotos por publicación del estudio.</param>
    /// <param name="ahora">Fecha y hora actual, en UTC.</param>
    public Foto AgregarFoto(int ancho, int alto, int maxFotos, DateTimeOffset ahora)
    {
        if (maxFotos <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(maxFotos), maxFotos, "El máximo de fotos tiene que ser mayor a 0.");

        FechaUtc.Validar(ahora, nameof(ahora));

        if (_fotos.Count >= maxFotos)
            throw new ReglaDeNegocioException(
                "RN-BLG-04", $"Una publicación puede tener hasta {maxFotos} fotos.");

        var foto = Foto.Crear(this, _fotos.Count, ancho, alto, ahora);
        _fotos.Add(foto);
        ActualizadaEn = ahora;

        return foto;
    }

    /// <summary>
    /// Quita una foto y corre las siguientes un lugar. Una publicación publicada no puede quedarse sin fotos
    /// (RN-BLG-04). Devuelve la foto quitada, para que el caso de uso borre sus archivos.
    /// </summary>
    public Foto QuitarFoto(Guid fotoId, DateTimeOffset ahora)
    {
        FechaUtc.Validar(ahora, nameof(ahora));

        var foto = BuscarFoto(fotoId);

        if (Estado == EstadoPublicacion.Publicada && _fotos.Count == 1)
            throw new ReglaDeNegocioException(
                "RN-BLG-04",
                "Una publicación publicada tiene que tener al menos una foto. Ocultala antes de quitar la última.");

        var ordenadas = _fotos.OrderBy(f => f.Orden).Where(f => f.Id != foto.Id).ToList();
        _fotos.Remove(foto);
        Renumerar(ordenadas);
        ActualizadaEn = ahora;

        return foto;
    }

    /// <summary>
    /// Pone la foto como portada: pasa a ser la primera y las demás se corren un lugar.
    /// </summary>
    public void ElegirPortada(Guid fotoId, DateTimeOffset ahora)
    {
        FechaUtc.Validar(ahora, nameof(ahora));

        var portada = BuscarFoto(fotoId);
        var ordenadas = _fotos.OrderBy(f => f.Orden).Where(f => f.Id != portada.Id).Prepend(portada).ToList();

        Renumerar(ordenadas);
        ActualizadaEn = ahora;
    }

    /// <summary>
    /// Ordena las fotos según la lista, que tiene que traer cada foto de la publicación una sola vez.
    /// La primera queda como portada.
    /// </summary>
    public void Reordenar(IReadOnlyList<Guid> fotoIds, DateTimeOffset ahora)
    {
        ArgumentNullException.ThrowIfNull(fotoIds);
        FechaUtc.Validar(ahora, nameof(ahora));

        var mismasFotos = fotoIds.Count == _fotos.Count
            && fotoIds.Distinct().Count() == fotoIds.Count
            && fotoIds.All(id => _fotos.Any(f => f.Id == id));

        if (!mismasFotos)
            throw new ArgumentException(
                "El orden tiene que incluir cada foto de la publicación una sola vez.", nameof(fotoIds));

        Renumerar([.. fotoIds.Select(id => _fotos.First(f => f.Id == id))]);
        ActualizadaEn = ahora;
    }

    /// <summary>
    /// Registra que el cliente dio su consentimiento para difundir las fotos, quién lo confirmó y cuándo
    /// (RN-BLG-03). Si ya estaba confirmado, no cambia nada: queda el primer registro.
    /// </summary>
    public void ConfirmarConsentimiento(Guid usuarioId, DateTimeOffset ahora)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El usuario que confirma no puede estar vacío.", nameof(usuarioId));

        FechaUtc.Validar(ahora, nameof(ahora));

        if (TieneConsentimiento)
            return;

        ConsentimientoConfirmadoPor = usuarioId;
        ConsentimientoConfirmadoEn = ahora;
        ActualizadaEn = ahora;
    }

    /// <summary>
    /// La muestra en el sitio público. Necesita al menos una foto (RN-BLG-04) y el consentimiento del cliente
    /// (RN-BLG-03). Si ya está publicada no cambia nada, así un doble toque en el celular no da error.
    /// </summary>
    public void Publicar(DateTimeOffset ahora)
    {
        FechaUtc.Validar(ahora, nameof(ahora));

        if (Estado == EstadoPublicacion.Publicada)
            return;

        if (_fotos.Count == 0)
            throw new ReglaDeNegocioException("RN-BLG-04", "Para publicar hace falta al menos una foto.");

        if (!TieneConsentimiento)
            throw new ReglaDeNegocioException(
                "RN-BLG-03",
                "Para publicar hay que confirmar que el cliente dio su consentimiento para difundir las fotos.");

        Estado = EstadoPublicacion.Publicada;
        PublicadaEn ??= ahora;
        ActualizadaEn = ahora;
    }

    /// <summary>
    /// La saca del sitio público sin borrar nada (RN-BLG-06). Un borrador también se puede ocultar, para
    /// archivarlo. Si ya está oculta no cambia nada.
    /// </summary>
    public void Ocultar(DateTimeOffset ahora)
    {
        FechaUtc.Validar(ahora, nameof(ahora));

        if (Estado == EstadoPublicacion.Oculta)
            return;

        Estado = EstadoPublicacion.Oculta;
        ActualizadaEn = ahora;
    }

    private void AplicarDatos(DatosPublicacion datos)
    {
        ArgumentNullException.ThrowIfNull(datos);

        var titulo = Texto.Obligatorio(datos.Titulo, MaxTitulo, "El título", nameof(datos.Titulo));
        var descripcion = Texto.Opcional(
            datos.Descripcion, MaxDescripcion, "La descripción", nameof(datos.Descripcion));

        ValidarId(datos.EstiloId, nameof(datos.EstiloId));
        ValidarId(datos.ZonaCuerpoId, nameof(datos.ZonaCuerpoId));
        ValidarId(datos.TatuadorId, nameof(datos.TatuadorId));

        if (!Enum.IsDefined(datos.Tipo))
            throw new ArgumentException("El tipo de publicación no es válido.", nameof(datos.Tipo));

        Titulo = titulo;
        Descripcion = descripcion;
        FechaTrabajo = datos.FechaTrabajo;
        EstiloId = datos.EstiloId;
        ZonaCuerpoId = datos.ZonaCuerpoId;
        TatuadorId = datos.TatuadorId;
        Destacada = datos.Destacada;
        Tipo = datos.Tipo;
    }

    /// <summary>
    /// Un Id opcional puede faltar (null), pero no venir vacío: Guid.Empty es un error de quien llama.
    /// </summary>
    private static void ValidarId(Guid? id, string nombreParametro)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "El Id no puede estar vacío: si no hay, tiene que ser null.", nombreParametro);
    }

    private Foto BuscarFoto(Guid fotoId) =>
        _fotos.FirstOrDefault(f => f.Id == fotoId)
        ?? throw new ArgumentException("La foto no pertenece a esta publicación.", nameof(fotoId));

    /// <summary>
    /// Vuelve a numerar las fotos desde 0, en el orden de la lista.
    /// </summary>
    private static void Renumerar(List<Foto> ordenadas)
    {
        for (var i = 0; i < ordenadas.Count; i++)
            ordenadas[i].CambiarOrden(i);
    }
}
