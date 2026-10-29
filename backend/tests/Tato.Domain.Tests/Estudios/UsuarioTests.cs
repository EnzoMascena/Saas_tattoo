namespace Tato.Domain.Tests.Estudios;

using Tato.Domain.Estudios;
using Xunit;

public class UsuarioTests
{
    [Fact]
    public void Crear_ConDatosValidos_DevuelveUsuario()
    {
        // Arrange
        var estudioId = Guid.NewGuid();

        // Act
        var usuario = Usuario.Crear(
            estudioId,
            "enzo@example.com",
            "Enzo",
            Rol.Duenio);

        // Assert
        Assert.NotEqual(Guid.Empty, usuario.Id);
        Assert.Equal(estudioId, usuario.EstudioId);
        Assert.Equal("enzo@example.com", usuario.Email);
        Assert.Equal("Enzo", usuario.Nombre);
        Assert.Equal(Rol.Duenio, usuario.Roles);
    }

    [Fact]
    public void Crear_ConEstudioIdVacio_LanzaArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Usuario.Crear(Guid.Empty, "enzo@example.com", "Enzo", Rol.Duenio));
        Assert.Contains("no puede estar vacío", ex.Message);
    }

    [Fact]
    public void Crear_ConEmailVacio_LanzaArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Usuario.Crear(Guid.NewGuid(), "", "Enzo", Rol.Duenio));
        Assert.Contains("no puede estar vacío", ex.Message);
    }

    [Fact]
    public void Crear_ConEmailMayorA256Caracteres_LanzaArgumentException()
    {
        // Arrange
        var emailLargo = new string('a', 250) + "@example.com";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Usuario.Crear(Guid.NewGuid(), emailLargo, "Enzo", Rol.Duenio));
        Assert.Contains("no puede exceder 256 caracteres", ex.Message);
    }

    [Fact]
    public void Crear_ConNombreVacio_LanzaArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Usuario.Crear(Guid.NewGuid(), "enzo@example.com", "", Rol.Duenio));
        Assert.Contains("no puede estar vacío", ex.Message);
    }

    [Fact]
    public void Crear_ConNombreMayorA100Caracteres_LanzaArgumentException()
    {
        // Arrange
        var nombreLargo = new string('a', 101);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Usuario.Crear(Guid.NewGuid(), "enzo@example.com", nombreLargo, Rol.Duenio));
        Assert.Contains("no puede exceder 100 caracteres", ex.Message);
    }

    [Fact]
    public void Crear_ConRolesSinNinguno_LanzaArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Usuario.Crear(Guid.NewGuid(), "enzo@example.com", "Enzo", (Rol)0));
        Assert.Contains("debe tener al menos un rol", ex.Message);
    }

    [Theory]
    [InlineData(Rol.Duenio)]
    [InlineData(Rol.Tatuador)]
    [InlineData(Rol.Administrativo)]
    [InlineData(Rol.Duenio | Rol.Tatuador)]
    [InlineData(Rol.Duenio | Rol.Administrativo)]
    [InlineData(Rol.Duenio | Rol.Tatuador | Rol.Administrativo)]
    public void Crear_ConRolesValidos_Exito(Rol roles)
    {
        // Act & Assert
        var usuario = Usuario.Crear(Guid.NewGuid(), "enzo@example.com", "Enzo", roles);
        Assert.Equal(roles, usuario.Roles);
    }

    [Fact]
    public void Crear_SinDuenio_ConTatuadorYAdministrativo_LanzaArgumentException()
    {
        // Arrange
        var roles = Rol.Tatuador | Rol.Administrativo;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Usuario.Crear(Guid.NewGuid(), "enzo@example.com", "Enzo", roles));
        Assert.Contains("no puede ser Tatuador y Administrativo a la vez", ex.Message);
    }

    [Fact]
    public void CambiarRoles_ConRolesValidos_Cambia()
    {
        // Arrange
        var usuario = Usuario.Crear(Guid.NewGuid(), "enzo@example.com", "Enzo", Rol.Duenio);

        // Act
        usuario.CambiarRoles(Rol.Duenio | Rol.Tatuador);

        // Assert
        Assert.Equal(Rol.Duenio | Rol.Tatuador, usuario.Roles);
    }

    [Fact]
    public void CambiarRoles_ConRolesInvalidos_LanzaArgumentException()
    {
        // Arrange
        var usuario = Usuario.Crear(Guid.NewGuid(), "enzo@example.com", "Enzo", Rol.Duenio);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            usuario.CambiarRoles(Rol.Tatuador | Rol.Administrativo));
        Assert.Contains("no puede ser Tatuador y Administrativo a la vez", ex.Message);
    }

    [Fact]
    public void Crear_ConRolDesconocido_LanzaArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Usuario.Crear(Guid.NewGuid(), "enzo@example.com", "Enzo", (Rol)8));
        Assert.Contains("rol desconocido", ex.Message);
    }

    [Fact]
    public void Crear_ConEspaciosAlrededor_GuardaLosTextosRecortados()
    {
        // Act
        var usuario = Usuario.Crear(Guid.NewGuid(), "  enzo@example.com  ", "  Enzo  ", Rol.Duenio);

        // Assert
        Assert.Equal("enzo@example.com", usuario.Email);
        Assert.Equal("Enzo", usuario.Nombre);
    }
}
