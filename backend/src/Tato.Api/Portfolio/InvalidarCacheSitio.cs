using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OutputCaching;

namespace Tato.Api.Portfolio;

/// <summary>
/// Vacía la caché del sitio público después de todo pedido que modifica datos y sale bien (D-57). Es un filtro de
/// toda la API, así ningún endpoint nuevo del panel se olvida de invalidar, y Application no se entera de la caché.
/// <para>
/// A esta escala vaciar todo es lo más simple: el sitio vuelve a leer de la base solo lo que se pide después.
/// shortcut: la caché es en memoria y de una sola instancia; con varias instancias, pasar a Redis (doc 06).
/// </para>
/// </summary>
internal sealed class InvalidarCacheSitio : IAsyncActionFilter
{
    private readonly IOutputCacheStore _cache;

    public InvalidarCacheSitio(IOutputCacheStore cache)
    {
        _cache = cache;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ejecutado = await next();

        if (HttpMethods.IsGet(context.HttpContext.Request.Method)
            || HttpMethods.IsHead(context.HttpContext.Request.Method)
            || ejecutado.Exception is not null)
            return;

        // Sin CancellationToken del pedido: el cambio ya se guardó, la caché se tiene que vaciar igual.
        await _cache.EvictByTagAsync(SitioController.EtiquetaCache, CancellationToken.None);
    }
}
