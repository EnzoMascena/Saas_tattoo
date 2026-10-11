using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Tato.Api.Comun;

/// <summary>
/// Exige el token antiforgery (doc 06, 7.1) en todo pedido de los controladores que modifica datos: POST, PUT,
/// PATCH y DELETE. Los GET no lo necesitan, y un endpoint puede quedar afuera con [IgnoreAntiforgeryToken]
/// (por ejemplo, un webhook que llama otro sistema).
/// <para>
/// Es propio porque lo que trae ASP.NET no sirve para una API JSON: AutoValidateAntiforgeryToken depende de MVC
/// con vistas, y el middleware UseAntiforgery solo revisa formularios HTML.
/// </para>
/// </summary>
internal sealed class ValidarAntiforgery : IAsyncAuthorizationFilter
{
    private readonly IAntiforgery _antiforgery;
    private readonly ProblemDetailsFactory _problemDetails;

    public ValidarAntiforgery(IAntiforgery antiforgery, ProblemDetailsFactory problemDetails)
    {
        _antiforgery = antiforgery;
        _problemDetails = problemDetails;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!ModificaDatos(context.HttpContext.Request.Method)
            || context.ActionDescriptor.EndpointMetadata.Any(m => m is IgnoreAntiforgeryTokenAttribute))
            return;

        if (await _antiforgery.IsRequestValidAsync(context.HttpContext))
            return;

        var problema = _problemDetails.CreateProblemDetails(
            context.HttpContext,
            StatusCodes.Status400BadRequest,
            title: "Falta el token antiforgery o ya no es válido.",
            detail: "Pedí uno nuevo en GET /api/auth/antiforgery y mandalo en el encabezado X-XSRF-TOKEN.");

        context.Result = new ObjectResult(problema) { StatusCode = problema.Status };
    }

    private static bool ModificaDatos(string metodo) =>
        HttpMethods.IsPost(metodo) || HttpMethods.IsPut(metodo)
        || HttpMethods.IsPatch(metodo) || HttpMethods.IsDelete(metodo);
}
