using Microsoft.Extensions.Options;
using NetVips;
using Tato.Application.Portfolio;

namespace Tato.Infrastructure.Portfolio;

/// <summary>
/// Adaptador de <see cref="IProcesadorFotos"/> con NetVips (libvips). Se eligió en lugar de ImageSharp por
/// su licencia, con el mismo criterio que la hoja «Evitar» del stack; además usa muy poca memoria.
/// </summary>
internal sealed class ProcesadorFotosNetVips : IProcesadorFotos
{
    private const int TamanioBuffer = 81_920;

    /// <summary>
    /// Formatos aceptados, según el cargador que elige libvips mirando el contenido del archivo y no su
    /// extensión: un .exe renombrado a .jpg no pasa.
    /// </summary>
    private static readonly HashSet<string> CargadoresAceptados =
        ["jpegload_buffer", "pngload_buffer", "webpload_buffer"];

    private readonly OpcionesFotos _opciones;

    public ProcesadorFotosNetVips(IOptions<OpcionesFotos> opciones)
    {
        _opciones = opciones.Value;
    }

    public async Task<FotoProcesada> ProcesarAsync(Stream original, CancellationToken cancellationToken)
    {
        var bytes = await LeerConLimiteAsync(original, cancellationToken);

        // libvips trabaja de forma sincrónica y usa mucha CPU: se corre fuera del hilo de la petición.
        return await Task.Run(() => Procesar(bytes), cancellationToken);
    }

    private FotoProcesada Procesar(byte[] bytes)
    {
        using var imagen = Cargar(bytes);

        if ((long)imagen.Width * imagen.Height > _opciones.MaxPixeles)
            throw new FotoInvalidaException(
                $"La foto tiene demasiados píxeles ({imagen.Width} × {imagen.Height}). " +
                $"El máximo es {_opciones.MaxPixeles / 1_000_000} megapíxeles.");

        try
        {
            // ThumbnailImage gira la foto según cómo se sacó (la orientación del EXIF) y no agranda las chicas.
            // CopyMemory decodifica la imagen completa: si el archivo está cortado, falla acá.
            using var detalle = imagen.ThumbnailImage(
                _opciones.LadoDetalle, height: _opciones.LadoDetalle, size: Enums.Size.Down);
            using var detalleEnMemoria = detalle.CopyMemory();

            // Las versiones chicas salen de la de detalle, que ya está girada y en memoria: es más rápido que
            // volver a decodificar el original.
            return new FotoProcesada(
            [
                Achicar(TamanioFoto.Miniatura, detalleEnMemoria, _opciones.LadoMiniatura),
                Achicar(TamanioFoto.Galeria, detalleEnMemoria, _opciones.LadoGaleria),
                Codificar(TamanioFoto.Detalle, detalleEnMemoria),
            ]);
        }
        catch (VipsException)
        {
            throw new FotoInvalidaException("La foto está dañada o incompleta. Probá subirla de nuevo.");
        }
    }

    /// <summary>
    /// Abre la imagen y controla que sea de un formato aceptado. Lee solo el encabezado: los píxeles se
    /// decodifican después, de a partes (acceso secuencial), para usar poca memoria.
    /// </summary>
    private static Image Cargar(byte[] bytes)
    {
        Image imagen;

        try
        {
            imagen = Image.NewFromBuffer(bytes, access: Enums.Access.Sequential, failOn: Enums.FailOn.Truncated);
        }
        catch (VipsException)
        {
            throw new FotoInvalidaException("El archivo no es una imagen.");
        }

        var cargador = imagen.Get("vips-loader") as string;

        if (cargador is null || !CargadoresAceptados.Contains(cargador))
        {
            imagen.Dispose();
            throw new FotoInvalidaException("Formato no aceptado. Subí la foto en JPEG, PNG o WebP.");
        }

        return imagen;
    }

    private VersionFoto Achicar(TamanioFoto tamanio, Image fuente, int lado)
    {
        using var achicada = fuente.ThumbnailImage(lado, height: lado, size: Enums.Size.Down);
        return Codificar(tamanio, achicada);
    }

    /// <summary>
    /// Guarda en WebP sin ningún metadato: ni EXIF, ni ubicación GPS, ni datos de la cámara.
    /// </summary>
    private VersionFoto Codificar(TamanioFoto tamanio, Image imagen) =>
        new(
            tamanio,
            imagen.WebpsaveBuffer(q: _opciones.CalidadWebp, keep: Enums.ForeignKeep.None),
            imagen.Width,
            imagen.Height);

    /// <summary>
    /// Lee la foto subida cortando apenas pasa el máximo: un archivo enorme no llega a ocupar memoria entero.
    /// </summary>
    private async Task<byte[]> LeerConLimiteAsync(Stream original, CancellationToken cancellationToken)
    {
        using var memoria = new MemoryStream();
        var buffer = new byte[TamanioBuffer];
        int leidos;

        while ((leidos = await original.ReadAsync(buffer, cancellationToken)) > 0)
        {
            if (memoria.Length + leidos > _opciones.MaxBytes)
                throw new FotoInvalidaException(
                    $"La foto pesa más de {_opciones.MaxBytes / 1024 / 1024} MB.");

            memoria.Write(buffer, 0, leidos);
        }

        return memoria.ToArray();
    }
}
