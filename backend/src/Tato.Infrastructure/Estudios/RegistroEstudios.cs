using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tato.Application.Estudios;
using Tato.Domain.Estudios;
using Tato.Infrastructure.Persistencia;

namespace Tato.Infrastructure.Estudios;

/// <summary>
/// Implementación de <see cref="IRegistroEstudios"/> con EF Core e Identity.
/// </summary>
internal sealed class RegistroEstudios : IRegistroEstudios
{
    private readonly TatoDbContext _contexto;
    private readonly UserManager<CuentaUsuario> _userManager;

    public RegistroEstudios(TatoDbContext contexto, UserManager<CuentaUsuario> userManager)
    {
        _contexto = contexto;
        _userManager = userManager;
    }

    public async Task AgregarEstudioAsync(
        Estudio estudio,
        IReadOnlyCollection<Estilo> estilos,
        IReadOnlyCollection<ZonaCuerpo> zonasCuerpo,
        CancellationToken cancellationToken)
    {
        // Un solo SaveChanges, que EF hace en una transacción: no puede quedar un estudio sin sus catálogos.
        _contexto.Estudios.Add(estudio);
        _contexto.Estilos.AddRange(estilos);
        _contexto.ZonasCuerpo.AddRange(zonasCuerpo);

        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExisteEstudioAsync(Guid estudioId, CancellationToken cancellationToken) =>
        _contexto.Estudios.AnyAsync(e => e.Id == estudioId, cancellationToken);

    public async Task<ResultadoAlta> AgregarUsuarioAsync(
        Usuario usuario,
        Tatuador? perfilTatuador,
        string contrasenia,
        CancellationToken cancellationToken)
    {
        // Se agregan sin guardar. CreateAsync valida la contraseña y la cuenta, y solo si está todo bien
        // guarda en un único SaveChanges el usuario, su perfil y la cuenta: una sola transacción.
        _contexto.Usuarios.Add(usuario);

        if (perfilTatuador is not null)
            _contexto.Tatuadores.Add(perfilTatuador);

        var resultado = await _userManager.CreateAsync(CuentaUsuario.Crear(usuario), contrasenia);

        if (resultado.Succeeded)
            return ResultadoAlta.Exitoso();

        // No se guardó nada. Se descartan los cambios pendientes para que un SaveChanges posterior no los guarde.
        _contexto.ChangeTracker.Clear();
        return ResultadoAlta.Fallido(resultado.Errors.Select(e => e.Description));
    }
}
