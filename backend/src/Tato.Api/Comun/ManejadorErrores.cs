using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tato.Application.Portfolio;
using Tato.Domain.Comun;

namespace Tato.Api.Comun;

/// <summary>
/// Convierte las excepciones en respuestas de error con el formato estándar ProblemDetails (RFC 9457), para que
/// el panel siempre reciba lo mismo: un código, un título y un mensaje que se le puede mostrar a la persona.
/// Los errores inesperados se registran en el log y no exponen ningún detalle interno.
/// </summary>
internal sealed class ManejadorErrores : IExceptionHandler
{
    private const int ErrorInterno = StatusCodes.Status500InternalServerError;

    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<ManejadorErrores> _logger;

    public ManejadorErrores(IProblemDetailsService problemDetails, ILogger<ManejadorErrores> logger)
    {
        _problemDetails = problemDetails;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problema = Traducir(exception);

        if (problema.Status == ErrorInterno)
            _logger.LogError(
                exception,
                "Error no controlado en {Metodo} {Ruta}",
                httpContext.Request.Method,
                httpContext.Request.Path);

        httpContext.Response.StatusCode = problema.Status ?? ErrorInterno;

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problema,
            Exception = exception,
        });
    }

    /// <summary>
    /// Decide el código y el mensaje de cada tipo de error. Es estático para poder probarlo sin levantar la API.
    /// </summary>
    internal static ProblemDetails Traducir(Exception excepcion) => excepcion switch
    {
        // La regla del negocio no permite la operación (por ejemplo, publicar sin consentimiento).
        // 422: el pedido está bien formado, pero no se puede hacer. El código de la regla va aparte,
        // para que el front reaccione a una regla puntual sin depender del texto.
        ReglaDeNegocioException regla => new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "La operación no cumple una regla del negocio.",
            Detail = regla.Message,
            Extensions = { ["regla"] = regla.Regla },
        },

        FotoInvalidaException foto => new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "La foto no se puede usar.",
            Detail = foto.Message,
        },

        // Datos mal formados que llegaron hasta el dominio. La validación de entrada tendría que frenarlos antes.
        ArgumentException argumento => new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Los datos enviados no son válidos.",
            Detail = SinNombreDeParametro(argumento),
        },

        _ => new ProblemDetails
        {
            Status = ErrorInterno,
            Title = "Ocurrió un error inesperado. Probá de nuevo en un momento.",
        },
    };

    /// <summary>
    /// .NET agrega « (Parameter 'nombre')» al final del mensaje: sirve para depurar, no para mostrar.
    /// </summary>
    private static string SinNombreDeParametro(ArgumentException excepcion)
    {
        var sufijo = $" (Parameter '{excepcion.ParamName}')";

        return excepcion.ParamName is not null && excepcion.Message.EndsWith(sufijo, StringComparison.Ordinal)
            ? excepcion.Message[..^sufijo.Length]
            : excepcion.Message;
    }
}
