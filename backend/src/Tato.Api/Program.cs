using System.Text.Json.Serialization;
using Tato.Api.Comun;
using Tato.Api.Estudios;
using Tato.Api.Portfolio;
using Tato.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Los comandos de consola (P-19) no levantan el servidor y usan la consola para pedir los datos:
// se apagan los logs para que no se mezclen con las preguntas.
var esComando = ComandosEstudios.EsComando(args);

if (esComando)
    builder.Logging.ClearProviders();

builder.Services.AddOpenApi(opciones => opciones.AddDocumentTransformer<DocumentarAntiforgery>());

builder.Services.AddControllers(opciones =>
    {
        // Todo POST, PUT, PATCH y DELETE de los controladores exige el token antiforgery (doc 06, 7.1).
        opciones.Filters.Add<ValidarAntiforgery>();

        // Y, si sale bien, vacía la caché del sitio público (D-57).
        opciones.Filters.Add<InvalidarCacheSitio>();
    })
    // Los enums viajan como texto ("Borrador"), no como número: el front no depende del orden del enum.
    .AddJsonOptions(opciones => opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// El documento de OpenAPI lee estas opciones y no las de AddJsonOptions: sin esto, los enums salen como números.
builder.Services.ConfigureHttpJsonOptions(opciones =>
    opciones.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Errores con formato ProblemDetails (RFC 9457): las excepciones las traduce ManejadorErrores.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorErrores>();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAutenticacion();
builder.Services.AddEstudios();
builder.Services.AddPortfolio(builder.Configuration);

var app = builder.Build();

if (esComando)
{
    Environment.ExitCode = await ComandosEstudios.EjecutarAsync(app.Services, args);
    return;
}

// Va primero: atrapa los errores de todo lo que viene después en el pipeline.
app.UseExceptionHandler();

// Las respuestas de error sin cuerpo (401, 403, 404) también salen como ProblemDetails.
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    // Por la política por defecto todo pide sesión: el documento de OpenAPI se abre explícitamente.
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(opciones => opciones.SwaggerEndpoint("/openapi/v1.json", "Tato API"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Después de la autorización: solo guarda lo que pasó todos los controles, y solo los endpoints con [OutputCache].
app.UseOutputCache();

app.MapControllers();

app.Run();
