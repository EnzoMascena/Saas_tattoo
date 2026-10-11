using System.Reflection;
using NetArchTest.Rules;
using Tato.Application.Estudios;
using Tato.Domain.Estudios;

namespace Tato.ArchitectureTests;

/// <summary>
/// Reglas de dependencia entre capas (D-37, README «Reglas que no se rompen»): las dependencias apuntan
/// siempre hacia el dominio, y el dominio no conoce nada externo.
/// </summary>
public class CapasTests
{
    private static readonly Assembly Domain = typeof(Estudio).Assembly;
    private static readonly Assembly Application = typeof(AltaEstudio).Assembly;
    private static readonly Assembly Infrastructure = typeof(Tato.Infrastructure.DependencyInjection).Assembly;

    [Fact]
    public void Domain_NoDependeDeOtrasCapasNiDeFrameworks()
    {
        var resultado = Types.InAssembly(Domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Tato.Application",
                "Tato.Infrastructure",
                "Tato.Api",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(resultado.IsSuccessful, Describir(resultado));
    }

    [Fact]
    public void Application_NoDependeDeInfrastructureNiDeApiNiDeEfCore()
    {
        var resultado = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Tato.Infrastructure",
                "Tato.Api",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(resultado.IsSuccessful, Describir(resultado));
    }

    [Fact]
    public void Infrastructure_NoDependeDeApi()
    {
        var resultado = Types.InAssembly(Infrastructure)
            .ShouldNot()
            .HaveDependencyOn("Tato.Api")
            .GetResult();

        Assert.True(resultado.IsSuccessful, Describir(resultado));
    }

    /// <summary>
    /// Si la regla falla, el mensaje dice qué tipos la rompen, para no tener que buscarlos a mano.
    /// </summary>
    private static string Describir(TestResult resultado) =>
        "Tipos que rompen la regla: " + string.Join(", ", resultado.FailingTypeNames ?? []);
}
