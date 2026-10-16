namespace Tato.Domain.Tests.Portfolio;

using Tato.Domain.Portfolio;
using Xunit;

/// <summary>
/// El tipo de una publicación (D-50) y el largo máximo de su texto (RN-BLG-11).
/// </summary>
public class PublicacionTipoTests
{
    private static readonly DateTimeOffset Ahora = PublicacionBuilder.Ahora;

    [Fact]
    public void Crear_SinTipo_EsUnTrabajo()
    {
        // Act
        var publicacion = new PublicacionBuilder().Build();

        // Assert
        Assert.Equal(TipoPublicacion.Trabajo, publicacion.Tipo);
    }

    [Fact]
    public void Crear_ComoBlog_GuardaElTipo()
    {
        // Arrange
        var datos = new DatosPublicacion("Mi viaje a Bolivia", Tipo: TipoPublicacion.Blog);

        // Act
        var publicacion = new PublicacionBuilder().ConDatos(datos).Build();

        // Assert
        Assert.Equal(TipoPublicacion.Blog, publicacion.Tipo);
    }

    [Fact]
    public void Editar_CambiaElTipo()
    {
        // Arrange: se creó como trabajo por error.
        var publicacion = new PublicacionBuilder().Build();
        var datos = new DatosPublicacion("Rosa en el antebrazo", Tipo: TipoPublicacion.Blog);

        // Act
        publicacion.Editar(datos, Ahora.AddHours(1));

        // Assert
        Assert.Equal(TipoPublicacion.Blog, publicacion.Tipo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    public void Crear_ConUnTipoQueNoExiste_LanzaArgumentException(int tipo)
    {
        // Arrange: un número que no es ningún tipo, como el que podría mandar un cliente con errores.
        var datos = new DatosPublicacion("Rosa", Tipo: (TipoPublicacion)tipo);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Publicacion.Crear(Guid.NewGuid(), Guid.NewGuid(), datos, Ahora));
        Assert.Contains("tipo de publicación no es válido", ex.Message);
    }

    [Fact]
    public void Crear_ConUnTextoDe2200Caracteres_LoGuarda()
    {
        // Arrange: el máximo exacto, como un texto largo copiado de Instagram.
        var texto = new string('a', 2200);
        var datos = new DatosPublicacion("Rosa", Descripcion: texto);

        // Act
        var publicacion = Publicacion.Crear(Guid.NewGuid(), Guid.NewGuid(), datos, Ahora);

        // Assert
        Assert.Equal(texto, publicacion.Descripcion);
    }
}
