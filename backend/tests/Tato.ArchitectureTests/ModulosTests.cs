using System.Reflection;
using NetArchTest.Rules;
using Tato.Domain.Estudios;

namespace Tato.ArchitectureTests;

/// <summary>
/// Fronteras entre módulos (D-38, README «Reglas que no se rompen»): un módulo no usa las clases de otro.
/// El compilador no lo puede controlar porque los módulos comparten proyecto; por eso lo controlan estos tests.
/// Lo que comparten todos los módulos vive en Comun.
/// </summary>
public class ModulosTests
{
    private static readonly Assembly Domain = typeof(Estudio).Assembly;
    private static readonly Assembly Infrastructure = typeof(Tato.Infrastructure.DependencyInjection).Assembly;

    [Fact]
    public void DominioDePortfolio_NoUsaClasesDeEstudios()
    {
        var resultado = Types.InAssembly(Domain)
            .That().ResideInNamespace("Tato.Domain.Portfolio")
            .ShouldNot().HaveDependencyOn("Tato.Domain.Estudios")
            .GetResult();

        Assert.True(resultado.IsSuccessful, Describir(resultado));
    }

    [Fact]
    public void DominioDeEstudios_NoUsaClasesDePortfolio()
    {
        var resultado = Types.InAssembly(Domain)
            .That().ResideInNamespace("Tato.Domain.Estudios")
            .ShouldNot().HaveDependencyOn("Tato.Domain.Portfolio")
            .GetResult();

        Assert.True(resultado.IsSuccessful, Describir(resultado));
    }

    [Fact]
    public void InfrastructureDePortfolio_NoUsaClasesDeEstudios()
    {
        var resultado = Types.InAssembly(Infrastructure)
            .That().ResideInNamespace("Tato.Infrastructure.Portfolio")
            .ShouldNot().HaveDependencyOnAny("Tato.Domain.Estudios", "Tato.Infrastructure.Estudios")
            .GetResult();

        Assert.True(resultado.IsSuccessful, Describir(resultado));
    }

    private static string Describir(TestResult resultado) =>
        "Tipos que rompen la regla: " + string.Join(", ", resultado.FailingTypeNames ?? []);
}
