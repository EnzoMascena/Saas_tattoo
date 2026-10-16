using Microsoft.Extensions.Options;
using NetVips;
using Tato.Application.Portfolio;
using Tato.Infrastructure.Portfolio;

namespace Tato.IntegrationTests.Portfolio;

/// <summary>
/// Procesamiento de fotos con libvips de verdad (RN-BLG-05). Las imágenes de prueba se generan en cada test.
/// </summary>
public sealed class ProcesadorFotosNetVipsTests
{
    private static readonly OpcionesFotos Opciones = new();

    [Fact]
    public async Task Procesar_JpegHorizontal_DevuelveLasTresVersionesEnWebp()
    {
        // Arrange: más grande que todas las versiones, así cada una sale con su lado máximo.
        var jpeg = Jpeg(2400, 1600);

        // Act
        var resultado = await ProcesarAsync(jpeg);

        // Assert
        Assert.Equal(3, resultado.Versiones.Count);
        AssertVersion(resultado, TamanioFoto.Miniatura, Opciones.LadoMiniatura);
        AssertVersion(resultado, TamanioFoto.Galeria, Opciones.LadoGaleria);
        AssertVersion(resultado, TamanioFoto.Detalle, Opciones.LadoDetalle);
    }

    [Fact]
    public async Task Procesar_FotoSacadaDeCostado_LaGuardaDerecha()
    {
        // Arrange: 1600 × 1200 acostada, con el EXIF que dice «girala 90°» (orientación 6).
        var jpeg = Jpeg(1600, 1200, orientacion: 6);

        // Act
        var detalle = (await ProcesarAsync(jpeg)).Detalle;

        // Assert: queda vertical, y como entra en 2048 no se achica.
        Assert.Equal(1200, detalle.Ancho);
        Assert.Equal(1600, detalle.Alto);
    }

    [Fact]
    public async Task Procesar_FotoConMetadatos_LasVersionesNoTienenNinguno()
    {
        // Arrange: el original sí tiene EXIF.
        var jpeg = Jpeg(1600, 1200, orientacion: 6);
        using (var original = Image.NewFromBuffer(jpeg))
            Assert.Contains("orientation", original.GetFields());

        // Act
        var resultado = await ProcesarAsync(jpeg);

        // Assert: ninguna versión conserva EXIF, orientación, perfil de color ni XMP.
        foreach (var version in resultado.Versiones)
        {
            using var leida = Image.NewFromBuffer(version.Contenido);
            Assert.DoesNotContain(leida.GetFields(), campo =>
                campo.StartsWith("exif") || campo == "orientation" || campo.StartsWith("icc")
                || campo.StartsWith("xmp"));
        }
    }

    [Fact]
    public async Task Procesar_FotoChica_NoLaAgranda()
    {
        // Act
        var resultado = await ProcesarAsync(Jpeg(300, 200));

        // Assert: más chica que la miniatura, así que todas las versiones quedan con su tamaño original.
        Assert.All(resultado.Versiones, version =>
        {
            Assert.Equal(300, version.Ancho);
            Assert.Equal(200, version.Alto);
        });
    }

    [Fact]
    public async Task Procesar_PngConTransparencia_SeAcepta()
    {
        // Arrange: PNG de 4 canales (con alfa).
        using var gris = Ruido(800, 600);
        using var conAlfa = gris.Bandjoin(gris, gris, gris);
        var png = conAlfa.PngsaveBuffer();

        // Act
        var resultado = await ProcesarAsync(png);

        // Assert
        Assert.Equal(800, resultado.Detalle.Ancho);
    }

    [Fact]
    public async Task Procesar_ArchivoQueNoEsImagen_LanzaFotoInvalida()
    {
        var texto = "esto no es una foto"u8.ToArray();

        var ex = await Assert.ThrowsAsync<FotoInvalidaException>(() => ProcesarAsync(texto));
        Assert.Contains("no es una imagen", ex.Message);
    }

    [Fact]
    public async Task Procesar_FormatoNoAceptado_LanzaFotoInvalida()
    {
        // Arrange: TIFF es una imagen válida, pero no un formato aceptado.
        using var gris = Ruido(300, 200);
        var tiff = gris.TiffsaveBuffer();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<FotoInvalidaException>(() => ProcesarAsync(tiff));
        Assert.Contains("JPEG, PNG o WebP", ex.Message);
    }

    [Fact]
    public async Task Procesar_JpegCortado_LanzaFotoInvalida()
    {
        // Arrange: como si la subida se hubiera cortado a la mitad.
        var jpeg = Jpeg(1600, 1200);
        var cortado = jpeg[..(jpeg.Length / 2)];

        // Act & Assert
        var ex = await Assert.ThrowsAsync<FotoInvalidaException>(() => ProcesarAsync(cortado));
        Assert.Contains("dañada o incompleta", ex.Message);
    }

    [Fact]
    public async Task Procesar_ArchivoMasPesadoQueElMaximo_LanzaFotoInvalida()
    {
        var opciones = new OpcionesFotos { MaxBytes = 1024 };

        var ex = await Assert.ThrowsAsync<FotoInvalidaException>(() => ProcesarAsync(Jpeg(300, 200), opciones));
        Assert.Contains("pesa más de", ex.Message);
    }

    [Fact]
    public async Task Procesar_MasPixelesQueElMaximo_LanzaFotoInvalida()
    {
        // Arrange: 400 × 300 = 120.000 píxeles, con un máximo de 100.000.
        var opciones = new OpcionesFotos { MaxPixeles = 100_000 };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<FotoInvalidaException>(() => ProcesarAsync(Jpeg(400, 300), opciones));
        Assert.Contains("demasiados píxeles", ex.Message);
    }

    private static Task<FotoProcesada> ProcesarAsync(byte[] archivo, OpcionesFotos? opciones = null)
    {
        var procesador = new ProcesadorFotosNetVips(Options.Create(opciones ?? Opciones));
        return procesador.ProcesarAsync(new MemoryStream(archivo), CancellationToken.None);
    }

    /// <summary>
    /// La versión existe, es WebP (empieza con «RIFF» y dice «WEBP» en el byte 8) y su lado mayor es el esperado.
    /// </summary>
    private static void AssertVersion(FotoProcesada resultado, TamanioFoto tamanio, int ladoMayor)
    {
        var version = Assert.Single(resultado.Versiones, v => v.Tamanio == tamanio);

        Assert.Equal("RIFF"u8.ToArray(), version.Contenido[..4]);
        Assert.Equal("WEBP"u8.ToArray(), version.Contenido[8..12]);
        Assert.Equal(ladoMayor, Math.Max(version.Ancho, version.Alto));
    }

    /// <summary>
    /// Imagen en escala de grises con píxeles al azar.
    /// </summary>
    private static Image Ruido(int ancho, int alto)
    {
        using var ruido = Image.Gaussnoise(ancho, alto, sigma: 40, mean: 128);
        return ruido.Cast(Enums.BandFormat.Uchar);
    }

    /// <summary>
    /// Foto JPEG en color con ruido. Si se indica, le agrega la orientación del EXIF.
    /// </summary>
    private static byte[] Jpeg(int ancho, int alto, int? orientacion = null)
    {
        using var gris = Ruido(ancho, alto);
        using var color = gris.Bandjoin(gris, gris);

        if (orientacion is null)
            return color.JpegsaveBuffer(q: 85);

        using var girada = color.Mutate(imagen => imagen.Set(GValue.GIntType, "orientation", orientacion.Value));
        return girada.JpegsaveBuffer(q: 85);
    }
}
