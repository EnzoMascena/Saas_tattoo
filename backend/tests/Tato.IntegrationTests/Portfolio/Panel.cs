using Tato.Application.Portfolio;
using Tato.Domain.Estudios;
using Tato.Domain.Portfolio;
using Tato.Infrastructure.Estudios;
using Tato.Infrastructure.Persistencia;
using Tato.Infrastructure.Portfolio;
using UsuarioDelEstudio = Tato.Domain.Estudios.Usuario;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// Arma los casos de uso del panel como lo hace la API, con el repositorio y las referencias reales, para un
/// estudio y un usuario. Cada operación usa un contexto nuevo, como cada pedido HTTP: así la concurrencia se
/// prueba de verdad, porque ningún contexto ve lo que tiene en memoria otro.
/// </summary>
internal sealed partial class Panel
{
    private readonly BaseDeDatosFixture _baseDeDatos;
    private readonly EstudioFalso _estudio;

    private Panel(
        BaseDeDatosFixture baseDeDatos,
        EstudioFalso estudio,
        UsuarioFalso usuario,
        AlmacenamientoEnMemoria almacenamiento)
    {
        _baseDeDatos = baseDeDatos;
        _estudio = estudio;
        Usuario = usuario;
        Almacenamiento = almacenamiento;
    }

    public Guid EstudioId => _estudio.EstudioId!.Value;

    public UsuarioFalso Usuario { get; }

    public AlmacenamientoEnMemoria Almacenamiento { get; }

    public RelojFijo Reloj { get; } = new(new DateTimeOffset(2026, 10, 16, 12, 0, 0, TimeSpan.Zero));

    /// <summary>
    /// Da de alta un estudio nuevo en la base y devuelve un panel de ese estudio para el usuario.
    /// </summary>
    /// <param name="almacenamiento">El de la API en memoria, para que lea las fotos que sube el panel.</param>
    public static async Task<Panel> CrearAsync(
        BaseDeDatosFixture baseDeDatos,
        UsuarioFalso usuario,
        int maxFotos = 10,
        AlmacenamientoEnMemoria? almacenamiento = null)
    {
        var estudio = Estudio.Crear($"Estudio {Guid.NewGuid():N}", "starter", maxFotosPorPublicacion: maxFotos);

        // Sin estudio en la sesión, como el comando de consola que da de alta estudios.
        await using (var contexto = baseDeDatos.CrearContexto(estudioId: null))
        {
            contexto.Estudios.Add(estudio);
            await contexto.SaveChangesAsync();
        }

        return new Panel(
            baseDeDatos, new EstudioFalso(estudio.Id), usuario, almacenamiento ?? new AlmacenamientoEnMemoria());
    }

    /// <summary>
    /// El mismo estudio, con otro usuario logueado.
    /// </summary>
    public Panel Como(UsuarioFalso usuario) => new(_baseDeDatos, _estudio, usuario, Almacenamiento);

    /// <summary>
    /// Da de alta en el estudio a un artista: un usuario con el rol Tatuador (y Dueño, si se pide) y su perfil,
    /// como lo hace el comando alta-usuario. Devuelve su sesión con el perfil, como la arma la cookie al iniciar
    /// sesión.
    /// </summary>
    public async Task<UsuarioFalso> CrearArtistaAsync(bool esDuenio = true)
    {
        var roles = esDuenio ? Rol.Duenio | Rol.Tatuador : Rol.Tatuador;
        var usuario = UsuarioDelEstudio.Crear(EstudioId, $"{Guid.NewGuid():N}@tato.test", "Artista de prueba", roles);
        var tatuador = Tatuador.Crear(usuario, $"Artista {Guid.NewGuid():N}");

        await using (var db = Contexto())
        {
            db.Usuarios.Add(usuario);
            db.Tatuadores.Add(tatuador);
            await db.SaveChangesAsync();
        }

        return new UsuarioFalso(usuario.Id, esDuenio, EsAdministrativo: false, TatuadorId: tatuador.Id);
    }

    public Task<Guid> CrearPublicacionAsync(string titulo) => CrearPublicacionAsync(new DatosPublicacion(titulo));

    public async Task<Guid> CrearPublicacionAsync(DatosPublicacion datos)
    {
        await using var db = Contexto();
        var crear = new CrearPublicacion(new RepositorioPublicaciones(db), Referencias(db), _estudio, Usuario, Reloj);
        return await crear.EjecutarAsync(datos);
    }

    /// <summary>
    /// Edita solo el título. Ojo: los demás datos quedan vacíos, incluida la página (la deja en la del estudio).
    /// </summary>
    public Task<string> EditarAsync(Guid id, string titulo, string version) =>
        EditarAsync(id, new DatosPublicacion(titulo), version);

    public async Task<string> EditarAsync(Guid id, DatosPublicacion datos, string version)
    {
        await using var db = Contexto();
        var editar = new EditarPublicacion(new RepositorioPublicaciones(db), Referencias(db), Usuario, Reloj);
        return await editar.EjecutarAsync(id, datos, version);
    }

    public async Task<string> ConfirmarConsentimientoAsync(Guid id, string version)
    {
        await using var db = Contexto();
        var acciones = new AccionesPublicacion(new RepositorioPublicaciones(db), Usuario, Reloj);
        return await acciones.ConfirmarConsentimientoAsync(id, version);
    }

    public async Task<string> PublicarAsync(Guid id, string version)
    {
        await using var db = Contexto();
        var acciones = new AccionesPublicacion(new RepositorioPublicaciones(db), Usuario, Reloj);
        return await acciones.PublicarAsync(id, version);
    }

    public async Task<string> OcultarAsync(Guid id, string version)
    {
        await using var db = Contexto();
        var acciones = new AccionesPublicacion(new RepositorioPublicaciones(db), Usuario, Reloj);
        return await acciones.OcultarAsync(id, version);
    }

    public async Task<FotoSubida> SubirFotoAsync(Guid id, string version)
    {
        await using var db = Contexto();
        var subir = new SubirFoto(
            new RepositorioPublicaciones(db), Referencias(db), new ProcesadorFalso(), Almacenamiento, Usuario, Reloj);
        return await subir.EjecutarAsync(id, Stream.Null, version);
    }

    public async Task<string> QuitarFotoAsync(Guid id, Guid fotoId, string version)
    {
        await using var db = Contexto();
        var acciones = new AccionesFotos(new RepositorioPublicaciones(db), Almacenamiento, Usuario, Reloj);
        return await acciones.QuitarAsync(id, fotoId, version);
    }

    /// <summary>
    /// Lee la publicación desde la base, con un contexto nuevo: lo que devuelve es lo que quedó guardado.
    /// </summary>
    public async Task<PublicacionDetalle> ObtenerAsync(Guid id)
    {
        await using var db = Contexto();
        return await new ConsultasPublicaciones(db).ObtenerAsync(id, CancellationToken.None)
            ?? throw new InvalidOperationException($"La publicación {id} no existe en el estudio {EstudioId}.");
    }

    private TatoDbContext Contexto() => _baseDeDatos.CrearContexto(EstudioId);

    private ReferenciasDelEstudio Referencias(TatoDbContext db) => new(db, _estudio);
}
