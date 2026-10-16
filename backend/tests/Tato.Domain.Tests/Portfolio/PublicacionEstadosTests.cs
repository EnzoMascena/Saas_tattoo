namespace Tato.Domain.Tests.Portfolio;

using Tato.Domain.Comun;
using Tato.Domain.Portfolio;
using Xunit;

/// <summary>
/// Consentimiento del cliente y cambios de estado: publicar y ocultar (RN-BLG-03, RN-BLG-04, RN-BLG-06).
/// </summary>
public class PublicacionEstadosTests
{
    private static readonly DateTimeOffset Ahora = PublicacionBuilder.Ahora;

    [Fact]
    public void ConfirmarConsentimiento_PrimeraVez_RegistraQuienYCuando()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().Build();
        var usuarioId = Guid.NewGuid();
        var despues = Ahora.AddMinutes(10);

        // Act
        publicacion.ConfirmarConsentimiento(usuarioId, despues);

        // Assert
        Assert.True(publicacion.TieneConsentimiento);
        Assert.Equal(usuarioId, publicacion.ConsentimientoConfirmadoPor);
        Assert.Equal(despues, publicacion.ConsentimientoConfirmadoEn);
        Assert.Equal(despues, publicacion.ActualizadaEn);
    }

    [Fact]
    public void ConfirmarConsentimiento_YaConfirmado_ConservaElPrimerRegistro()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().Build();
        var primerUsuario = Guid.NewGuid();
        publicacion.ConfirmarConsentimiento(primerUsuario, Ahora);

        // Act
        publicacion.ConfirmarConsentimiento(Guid.NewGuid(), Ahora.AddDays(1));

        // Assert
        Assert.Equal(primerUsuario, publicacion.ConsentimientoConfirmadoPor);
        Assert.Equal(Ahora, publicacion.ConsentimientoConfirmadoEn);
        Assert.Equal(Ahora, publicacion.ActualizadaEn);
    }

    [Fact]
    public void ConfirmarConsentimiento_ConUsuarioVacio_LanzaArgumentException()
    {
        var publicacion = new PublicacionBuilder().Build();

        Assert.Throws<ArgumentException>(() => publicacion.ConfirmarConsentimiento(Guid.Empty, Ahora));
        Assert.False(publicacion.TieneConsentimiento);
    }

    [Fact]
    public void Publicar_ConFotoYConsentimiento_QuedaPublicada()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().ConFotos(1).ConConsentimiento().Build();
        var despues = Ahora.AddHours(1);

        // Act
        publicacion.Publicar(despues);

        // Assert
        Assert.Equal(EstadoPublicacion.Publicada, publicacion.Estado);
        Assert.Equal(despues, publicacion.PublicadaEn);
        Assert.Equal(despues, publicacion.ActualizadaEn);
    }

    [Fact]
    public void Publicar_SinFotos_LanzaReglaDeNegocioYSigueEnBorrador()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().ConConsentimiento().Build();

        // Act & Assert
        var ex = Assert.Throws<ReglaDeNegocioException>(() => publicacion.Publicar(Ahora));
        Assert.Equal("RN-BLG-04", ex.Regla);
        Assert.Equal(EstadoPublicacion.Borrador, publicacion.Estado);
        Assert.Null(publicacion.PublicadaEn);
    }

    [Fact]
    public void Publicar_SinConsentimiento_LanzaReglaDeNegocioYSigueEnBorrador()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().ConFotos(1).Build();

        // Act & Assert
        var ex = Assert.Throws<ReglaDeNegocioException>(() => publicacion.Publicar(Ahora));
        Assert.Equal("RN-BLG-03", ex.Regla);
        Assert.Contains("consentimiento", ex.Message);
        Assert.Equal(EstadoPublicacion.Borrador, publicacion.Estado);
    }

    [Fact]
    public void Publicar_YaPublicada_NoCambiaNada()
    {
        // Un doble toque en el celular no tiene que dar error ni mover las fechas.
        var publicacion = new PublicacionBuilder().Publicada().Build();

        publicacion.Publicar(Ahora.AddHours(1));

        Assert.Equal(EstadoPublicacion.Publicada, publicacion.Estado);
        Assert.Equal(Ahora, publicacion.PublicadaEn);
        Assert.Equal(Ahora, publicacion.ActualizadaEn);
    }

    [Fact]
    public void Ocultar_Publicada_QuedaOcultaSinPerderNada()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().Publicada().ConFotos(2).Build();
        var despues = Ahora.AddDays(1);

        // Act
        publicacion.Ocultar(despues);

        // Assert: ocultar no borra (RN-BLG-06).
        Assert.Equal(EstadoPublicacion.Oculta, publicacion.Estado);
        Assert.Equal(2, publicacion.Fotos.Count);
        Assert.True(publicacion.TieneConsentimiento);
        Assert.Equal(Ahora, publicacion.PublicadaEn);
        Assert.Equal(despues, publicacion.ActualizadaEn);
    }

    [Fact]
    public void Ocultar_Borrador_QuedaArchivado()
    {
        var publicacion = new PublicacionBuilder().Build();

        publicacion.Ocultar(Ahora);

        Assert.Equal(EstadoPublicacion.Oculta, publicacion.Estado);
        Assert.Null(publicacion.PublicadaEn);
    }

    [Fact]
    public void Ocultar_YaOculta_NoCambiaNada()
    {
        var publicacion = new PublicacionBuilder().Oculta().Build();

        publicacion.Ocultar(Ahora.AddHours(1));

        Assert.Equal(EstadoPublicacion.Oculta, publicacion.Estado);
        Assert.Equal(Ahora, publicacion.ActualizadaEn);
    }

    [Fact]
    public void Publicar_OcultaQueYaSePublico_ConservaLaFechaDeLaPrimeraPublicacion()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().Publicada().Build();
        publicacion.Ocultar(Ahora.AddDays(1));

        // Act
        publicacion.Publicar(Ahora.AddDays(2));

        // Assert
        Assert.Equal(EstadoPublicacion.Publicada, publicacion.Estado);
        Assert.Equal(Ahora, publicacion.PublicadaEn);
        Assert.Equal(Ahora.AddDays(2), publicacion.ActualizadaEn);
    }

    [Fact]
    public void Publicar_OcultaQueSeQuedoSinFotos_LanzaReglaDeNegocio()
    {
        // Arrange
        var publicacion = new PublicacionBuilder().Publicada().Build();
        publicacion.Ocultar(Ahora);
        publicacion.QuitarFoto(publicacion.Fotos[0].Id, Ahora);

        // Act & Assert
        var ex = Assert.Throws<ReglaDeNegocioException>(() => publicacion.Publicar(Ahora));
        Assert.Equal("RN-BLG-04", ex.Regla);
        Assert.Equal(EstadoPublicacion.Oculta, publicacion.Estado);
    }

    [Fact]
    public void Publicar_ConFechaQueNoEsUtc_LanzaArgumentException()
    {
        var publicacion = new PublicacionBuilder().ConFotos(1).ConConsentimiento().Build();

        Assert.Throws<ArgumentException>(() => publicacion.Publicar(Ahora.ToOffset(TimeSpan.FromHours(-3))));
        Assert.Equal(EstadoPublicacion.Borrador, publicacion.Estado);
    }
}
