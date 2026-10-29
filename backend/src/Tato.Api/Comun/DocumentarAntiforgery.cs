using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Tato.Api.Estudios;

namespace Tato.Api.Comun;

/// <summary>
/// Le avisa a Swagger que los pedidos llevan el token antiforgery (doc 06, 7.1),
/// así muestra el botón «Authorize» para cargarlo.
/// </summary>
internal sealed class DocumentarAntiforgery : IOpenApiDocumentTransformer
{
    private const string Esquema = "Antiforgery";

    public Task TransformAsync(
        OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[Esquema] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = AutenticacionExtensions.EncabezadoAntiforgery,
            Description = "Token de GET /api/auth/antiforgery. Pedilo de nuevo después de iniciar o cerrar sesión.",
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(Esquema, document)] = [],
        });

        return Task.CompletedTask;
    }
}
