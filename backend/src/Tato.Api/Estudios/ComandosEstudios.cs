using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Tato.Application.Estudios;
using Tato.Domain.Estudios;

namespace Tato.Api.Estudios;

/// <summary>
/// Comandos de consola para dar de alta estudios y usuarios (P-19). En la versión 1 no hay registro ni
/// invitaciones: los corre el desarrollador, en su máquina o en el servidor. Desde la carpeta backend:
/// <code>
/// dotnet run --project src/Tato.Api -- alta-estudio
/// dotnet run --project src/Tato.Api -- alta-usuario
/// </code>
/// </summary>
public static class ComandosEstudios
{
    private const string ComandoAltaEstudio = "alta-estudio";
    private const string ComandoAltaUsuario = "alta-usuario";

    public static bool EsComando(string[] args) =>
        args.Length > 0 && args[0] is ComandoAltaEstudio or ComandoAltaUsuario;

    /// <summary>
    /// Ejecuta el comando y devuelve el código de salida: 0 si salió bien, 1 si no.
    /// </summary>
    public static async Task<int> EjecutarAsync(IServiceProvider servicios, string[] args)
    {
        await using var alcance = servicios.CreateAsyncScope();

        try
        {
            return args[0] == ComandoAltaEstudio
                ? await AltaEstudioAsync(alcance.ServiceProvider)
                : await AltaUsuarioAsync(alcance.ServiceProvider);
        }
        catch (ArgumentException ex)
        {
            // Las validaciones del dominio llegan como ArgumentException: se muestra el motivo.
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> AltaEstudioAsync(IServiceProvider servicios)
    {
        Console.WriteLine("Alta de estudio");

        var nombre = Preguntar("Nombre del estudio");
        var plan = Preguntar("Plan (por ejemplo, piloto)");
        var zonaHoraria = PreguntarOpcional($"Zona horaria (Enter para {Estudio.ZonaHorariaPorDefecto})");

        var id = await servicios.GetRequiredService<AltaEstudio>().EjecutarAsync(nombre, plan, zonaHoraria);

        Console.WriteLine($"Estudio creado. Id: {id}");
        Console.WriteLine("Guardá el Id: lo pide alta-usuario.");
        return 0;
    }

    private static async Task<int> AltaUsuarioAsync(IServiceProvider servicios)
    {
        Console.WriteLine("Alta de usuario");

        if (!Guid.TryParse(Preguntar("Id del estudio"), out var estudioId))
        {
            Console.Error.WriteLine("Error: el Id del estudio no tiene formato de UUID.");
            return 1;
        }

        var email = Preguntar("Email");
        var nombre = Preguntar("Nombre");

        // Enum.TryParse acepta varios roles separados por coma, por ejemplo "Duenio, Tatuador".
        var textoRoles = Preguntar("Roles (Duenio, Tatuador, Administrativo; separados por coma)");

        if (!Enum.TryParse<Rol>(textoRoles, ignoreCase: true, out var roles))
        {
            Console.Error.WriteLine("Error: no se reconocen los roles.");
            return 1;
        }

        string? nombreArtistico = null;
        string? instagram = null;

        if (roles.HasFlag(Rol.Tatuador))
        {
            nombreArtistico = Preguntar("Nombre artístico");
            instagram = PreguntarOpcional("Instagram (Enter para omitir)");
        }

        // El largo mínimo sale de la configuración de Identity, no de un número repetido acá.
        var largoMinimo = servicios.GetRequiredService<IOptions<IdentityOptions>>().Value.Password.RequiredLength;
        var contrasenia = LeerOculto($"Contraseña (mínimo {largoMinimo} caracteres)");

        if (contrasenia != LeerOculto("Repetí la contraseña"))
        {
            Console.Error.WriteLine("Error: las contraseñas no coinciden.");
            return 1;
        }

        var solicitud = new SolicitudAltaUsuario(estudioId, email, nombre, roles, nombreArtistico, instagram);
        var resultado = await servicios.GetRequiredService<AltaUsuario>().EjecutarAsync(solicitud, contrasenia);

        if (!resultado.Exito)
        {
            foreach (var error in resultado.Errores)
                Console.Error.WriteLine($"Error: {error}");

            return 1;
        }

        Console.WriteLine("Usuario creado: ya puede iniciar sesión.");
        return 0;
    }

    private static string Preguntar(string etiqueta)
    {
        Console.Write($"{etiqueta}: ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    private static string? PreguntarOpcional(string etiqueta)
    {
        var respuesta = Preguntar(etiqueta);
        return respuesta.Length == 0 ? null : respuesta;
    }

    /// <summary>
    /// Lee un texto sin mostrarlo en pantalla. Si la entrada viene de un archivo o de un pipe, la lee normal.
    /// </summary>
    private static string LeerOculto(string etiqueta)
    {
        Console.Write($"{etiqueta}: ");

        if (Console.IsInputRedirected)
            return Console.ReadLine() ?? string.Empty;

        var texto = new StringBuilder();

        while (true)
        {
            var tecla = Console.ReadKey(intercept: true);

            if (tecla.Key == ConsoleKey.Enter)
                break;

            if (tecla.Key == ConsoleKey.Backspace)
            {
                if (texto.Length > 0)
                    texto.Length--;

                continue;
            }

            if (!char.IsControl(tecla.KeyChar))
                texto.Append(tecla.KeyChar);
        }

        Console.WriteLine();
        return texto.ToString();
    }
}
