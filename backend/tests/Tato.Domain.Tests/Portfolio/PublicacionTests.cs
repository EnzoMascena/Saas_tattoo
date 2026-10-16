namespace Tato.Domain.Tests.Portfolio;

using Tato.Domain.Portfolio;
using Xunit;

/// <summary>
/// Crear y editar los datos de una publicación.
/// </summary>
public class PublicacionTests
{
    private static readonly DateTimeOffset Ahora = PublicacionBuilder.Ahora;

    [Fact]
    public void Crear_ConDatosValidos_DevuelveBorrador()
    {
        // Arrange
        var estudioId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var estiloId = Guid.NewGuid();
        var zonaId = Guid.NewGuid();
        var tatuadorId = Guid.NewGuid();
        var datos = new DatosPublicacion(
            "  Rosa en el antebrazo  ",
            "Fineline con sombras suaves.",
            new DateOnly(2026, 9, 20),
            estiloId,
            zonaId,
            tatuadorId,
            Destacada: true);

        // Act
        var publicacion = Publicacion.Crear(estudioId, usuarioId, datos, Ahora);

        // Assert
        Assert.NotEqual(Guid.Empty, publicacion.Id);
        Assert.Equal(estudioId, publicacion.EstudioId);
        Assert.Equal("Rosa en el antebrazo", publicacion.Titulo);
        Assert.Equal("Fineline con sombras suaves.", publicacion.Descripcion);
        Assert.Equal(new DateOnly(2026, 9, 20), publicacion.FechaTrabajo);
        Assert.Equal(estiloId, publicacion.EstiloId);
        Assert.Equal(zonaId, publicacion.ZonaCuerpoId);
        Assert.Equal(tatuadorId, publicacion.TatuadorId);
        Assert.True(publicacion.Destacada);
        Assert.Equal(EstadoPublicacion.Borrador, publicacion.Estado);
        Assert.Equal(usuarioId, publicacion.CreadaPor);
        Assert.Equal(Ahora, publicacion.CreadaEn);
        Assert.Equal(Ahora, publicacion.ActualizadaEn);
        Assert.Null(publicacion.PublicadaEn);
        Assert.False(publicacion.TieneConsentimiento);
        Assert.Empty(publicacion.Fotos);
        Assert.Null(publicacion.Portada);
    }

    [Fact]
    public void Crear_SoloConTitulo_DejaLoOpcionalVacio()
    {
        // Act
        var publicacion = Publicacion.Crear(
            Guid.NewGuid(), Guid.NewGuid(), new DatosPublicacion("Rosa", Descripcion: "   "), Ahora);

        // Assert
        Assert.Null(publicacion.Descripcion);
        Assert.Null(publicacion.FechaTrabajo);
        Assert.Null(publicacion.EstiloId);
        Assert.Null(publicacion.ZonaCuerpoId);
        Assert.Null(publicacion.TatuadorId);
        Assert.False(publicacion.Destacada);
    }

    [Fact]
    public void Crear_ConEstudioVacio_LanzaArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Publicacion.Crear(Guid.Empty, Guid.NewGuid(), new DatosPublicacion("Rosa"), Ahora));
        Assert.Contains("EstudioId no puede estar vacío", ex.Message);
    }

    [Fact]
    public void Crear_ConUsuarioVacio_LanzaArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Publicacion.Crear(Guid.NewGuid(), Guid.Empty, new DatosPublicacion("Rosa"), Ahora));
    }

    [Fact]
    public void Crear_ConDatosNull_LanzaArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Publicacion.Crear(Guid.NewGuid(), Guid.NewGuid(), null!, Ahora));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConTituloVacio_LanzaArgumentException(string titulo)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Publicacion.Crear(Guid.NewGuid(), Guid.NewGuid(), new DatosPublicacion(titulo), Ahora));
        Assert.Contains("título no puede estar vacío", ex.Message);
    }

    [Fact]
    public void Crear_ConTituloMayorA120Caracteres_LanzaArgumentException()
    {
        // Arrange
        var datos = new DatosPublicacion(new string('a', 121));

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Publicacion.Crear(Guid.NewGuid(), Guid.NewGuid(), datos, Ahora));
        Assert.Contains("no puede exceder 120 caracteres", ex.Message);
    }

    [Fact]
    public void Crear_ConDescripcionMayorA2200Caracteres_LanzaArgumentException()
    {
        // Arrange
        var datos = new DatosPublicacion("Rosa", Descripcion: new string('a', 2201));

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Publicacion.Crear(Guid.NewGuid(), Guid.NewGuid(), datos, Ahora));
        Assert.Contains("no puede exceder 2200 caracteres", ex.Message);
    }

    [Fact]
    public void Crear_ConEstiloIdVacio_LanzaArgumentException()
    {
        // Guid.Empty no es «sin estilo»: para eso está null.
        var datos = new DatosPublicacion("Rosa", EstiloId: Guid.Empty);

        Assert.Throws<ArgumentException>(() => Publicacion.Crear(Guid.NewGuid(), Guid.NewGuid(), datos, Ahora));
    }

    [Fact]
    public void Crear_ConZonaIdVacio_LanzaArgumentException()
    {
        var datos = new DatosPublicacion("Rosa", ZonaCuerpoId: Guid.Empty);

        Assert.Throws<ArgumentException>(() => Publicacion.Crear(Guid.NewGuid(), Guid.NewGuid(), datos, Ahora));
    }

    [Fact]
    public void Crear_ConTatuadorIdVacio_LanzaArgumentException()
    {
        var datos = new DatosPublicacion("Rosa", TatuadorId: Guid.Empty);

        Assert.Throws<ArgumentException>(() => Publicacion.Crear(Guid.NewGuid(), Guid.NewGuid(), datos, Ahora));
    }

    [Fact]
    public void Crear_ConFechaQueNoEsUtc_LanzaArgumentException()
    {
        // Arrange: la misma hora, pero con el desfase de Argentina.
        var ahoraArgentina = new DateTimeOffset(2026, 10, 11, 12, 0, 0, TimeSpan.FromHours(-3));

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Publicacion.Crear(Guid.NewGuid(), Guid.NewGuid(), new DatosPublicacion("Rosa"), ahoraArgentina));
        Assert.Contains("UTC", ex.Message);
    }

    [Fact]
    public void Editar_ConDatosValidos_CambiaLosDatosYLaFechaDeActualizacion()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().Build();
        var despues = Ahora.AddHours(1);
        var estiloId = Guid.NewGuid();

        // Act
        publicacion.Editar(
            new DatosPublicacion("Rosa y daga", "Nueva descripción", EstiloId: estiloId, Destacada: true), despues);

        // Assert
        Assert.Equal("Rosa y daga", publicacion.Titulo);
        Assert.Equal("Nueva descripción", publicacion.Descripcion);
        Assert.Equal(estiloId, publicacion.EstiloId);
        Assert.True(publicacion.Destacada);
        Assert.Equal(despues, publicacion.ActualizadaEn);
        Assert.Equal(Ahora, publicacion.CreadaEn);
    }

    [Fact]
    public void Editar_SinEstilo_LoQuita()
    {
        // Arrange
        var publicacion = new PublicacionBuilder()
            .ConDatos(new DatosPublicacion("Rosa", EstiloId: Guid.NewGuid()))
            .Build();

        // Act
        publicacion.Editar(new DatosPublicacion("Rosa"), Ahora);

        // Assert
        Assert.Null(publicacion.EstiloId);
    }

    [Fact]
    public void Editar_PublicacionPublicada_SiguePublicada()
    {
        // Corregir un trabajo publicado no obliga a ocultarlo.
        var publicacion = new PublicacionBuilder().Publicada().Build();

        publicacion.Editar(new DatosPublicacion("Título corregido"), Ahora.AddDays(1));

        Assert.Equal("Título corregido", publicacion.Titulo);
        Assert.Equal(EstadoPublicacion.Publicada, publicacion.Estado);
    }

    [Fact]
    public void Editar_ConUnDatoInvalido_NoCambiaNada()
    {
        // Arrange: el título es válido, pero la zona no. No tiene que quedar a medio editar.
        var publicacion = new PublicacionBuilder().ConDatos(new DatosPublicacion("Rosa")).Build();

        // Act
        Assert.Throws<ArgumentException>(() =>
            publicacion.Editar(new DatosPublicacion("Otro título", ZonaCuerpoId: Guid.Empty), Ahora.AddHours(1)));

        // Assert
        Assert.Equal("Rosa", publicacion.Titulo);
        Assert.Equal(Ahora, publicacion.ActualizadaEn);
    }
}
