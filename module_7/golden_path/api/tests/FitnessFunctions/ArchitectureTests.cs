using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using Assembly = System.Reflection.Assembly;

namespace FitnessFunctions;

public class ArchitectureTests
{
    [Theory]
    [InlineData("Core", new[] { "Infrastructure", "Api"})]
    [InlineData("Infrastructure", new[] { "Api" })]
    public void ModuleToModuleDependencies_ShouldOnlyBeViaDataTransferLibraries(string assemblyUnderTest, string[] compareAgainst)
    {
        foreach (var assembly in compareAgainst)
        {
            var result = Types.InAssembly(Assembly.Load(assemblyUnderTest))
                .That()
                .ResideInNamespace(assemblyUnderTest)
                .ShouldNot()
                .HaveDependencyOn(assembly)
                .GetResult();

            var failingTypes = String.Join(',', (result.FailingTypes ?? []).Select(type => type.FullName).ToArray());

            result.IsSuccessful
                .Should()
                .BeTrue($"{assemblyUnderTest} should not have a dependency on {assembly}. Failing types: {failingTypes}");
        }
    }
    [Theory]
    [InlineData("Core")]
    public void CoreLibraries_ShouldOnlyDependOnThemselves(string assemblyUnderTest)
    {
        var result = Types.InAssembly(Assembly.Load(assemblyUnderTest))
            .That()
            .ResideInNamespace(assemblyUnderTest)
            .Should()
            .OnlyHaveDependenciesOn(assemblyUnderTest, "PlantBasedPizza.Shared", "FluentValidation", "System", "Microsoft")
            .GetResult();

        var failingTypes = String.Join(',', (result.FailingTypes ?? []).Select(type => type.FullName).ToArray());

        result.IsSuccessful
            .Should()
            .BeTrue($"{assemblyUnderTest} should only hvae a dependency on itself. Failing types: {failingTypes}");
    }
}