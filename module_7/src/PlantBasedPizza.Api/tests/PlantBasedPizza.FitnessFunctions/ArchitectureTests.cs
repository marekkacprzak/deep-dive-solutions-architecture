using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using Assembly = System.Reflection.Assembly;

namespace PlantBasedPizza.FitnessFunctions;

public class ArchitectureTests
{
    [Theory]
    [InlineData("PlantBasedPizza.OrderManager.Core", new[] { "PlantBasedPizza.Recipes.Core", "PlantBasedPizza.Kitchen.Core", "PlantBasedPizza.Deliver.Core", "PlantBasedPizza.Payment.Core", "PlantBasedPizza.Payment.Infrastructure", "PlantBasedPizza.Recipes.Infrastructure", "PlantBasedPizza.Kitchen.Infrastructure", "PlantBasedPizza.Deliver.Infrastructure" })]
    [InlineData("PlantBasedPizza.Recipes.Core", new[] { "PlantBasedPizza.OrderManager.Core", "PlantBasedPizza.Kitchen.Core", "PlantBasedPizza.Deliver.Core", "PlantBasedPizza.Payment.Core", "PlantBasedPizza.Payment.Infrastructure", "PlantBasedPizza.OrderManager.Infrastructure", "PlantBasedPizza.Kitchen.Infrastructure", "PlantBasedPizza.Deliver.Infrastructure" })]
    [InlineData("PlantBasedPizza.Kitchen.Core", new[] { "PlantBasedPizza.Recipes.Core", "PlantBasedPizza.OrderManager.Core", "PlantBasedPizza.Deliver.Core", "PlantBasedPizza.Payment.Core", "PlantBasedPizza.Payment.Infrastructure", "PlantBasedPizza.Recipes.Infrastructure", "PlantBasedPizza.OrderManager.Infrastructure", "PlantBasedPizza.Deliver.Infrastructure" })]
    [InlineData("PlantBasedPizza.Deliver.Core", new[] { "PlantBasedPizza.Recipes.Core", "PlantBasedPizza.Kitchen.Core", "PlantBasedPizza.OrderManager.Core", "PlantBasedPizza.Payment.Core", "PlantBasedPizza.Payment.Infrastructure", "PlantBasedPizza.Recipes.Infrastructure", "PlantBasedPizza.Kitchen.Infrastructure", "PlantBasedPizza.OrderManager.Infrastructure" })]
    [InlineData("PlantBasedPizza.OrderManager.Infrastructure", new[] { "PlantBasedPizza.Recipes.Core", "PlantBasedPizza.Kitchen.Core", "PlantBasedPizza.Deliver.Core", "PlantBasedPizza.Recipes.Infrastructure", "PlantBasedPizza.Payment.Infrastructure", "PlantBasedPizza.Kitchen.Infrastructure", "PlantBasedPizza.Deliver.Infrastructure" })]
    [InlineData("PlantBasedPizza.Recipes.Infrastructure", new[] { "PlantBasedPizza.OrderManager.Core", "PlantBasedPizza.Kitchen.Core", "PlantBasedPizza.Deliver.Core", "PlantBasedPizza.OrderManager.Infrastructure", "PlantBasedPizza.Payment.Core", "PlantBasedPizza.Payment.Infrastructure", "PlantBasedPizza.Kitchen.Infrastructure", "PlantBasedPizza.Deliver.Infrastructure" })]
    [InlineData("PlantBasedPizza.Kitchen.Infrastructure", new[] { "PlantBasedPizza.Recipes.Core", "PlantBasedPizza.OrderManager.Core", "PlantBasedPizza.Deliver.Core", "PlantBasedPizza.Recipes.Infrastructure", "PlantBasedPizza.Payment.Core", "PlantBasedPizza.Payment.Infrastructure", "PlantBasedPizza.OrderManager.Infrastructure", "PlantBasedPizza.Deliver.Infrastructure" })]
    [InlineData("PlantBasedPizza.Deliver.Infrastructure", new[] { "PlantBasedPizza.Recipes.Core", "PlantBasedPizza.Kitchen.Core", "PlantBasedPizza.OrderManager.Core", "PlantBasedPizza.Recipes.Infrastructure", "PlantBasedPizza.Payment.Core", "PlantBasedPizza.Payment.Infrastructure", "PlantBasedPizza.Kitchen.Infrastructure", "PlantBasedPizza.OrderManager.Infrastructure" })]
    public void ModuleToModuleDependencies_ShouldOnlyBeViaDataTransferLibraries(string assemblyUnderTest, string[] compareAgainst)
    {
        foreach (var assembly in compareAgainst)
        {
            var result = Types.InAssembly(Assembly.Load(assemblyUnderTest))
                .That()
                .ResideInNamespace(assemblyUnderTest)
                .And()
                .DoNotResideInNamespace("PlantBasedPizza.OrderManager.Infrastructure.AWS")
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
    [InlineData("PlantBasedPizza.OrderManager.Core")]
    [InlineData("PlantBasedPizza.Recipes.Core")]
    [InlineData("PlantBasedPizza.Kitchen.Core")]
    [InlineData("PlantBasedPizza.Deliver.Core")]
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

    [Fact]
    public void OrderManagerService_ShouldDependOnPaymentDataTransferLibrary()
    {
        var assemblyUnderTest = "PlantBasedPizza.OrderManager.Infrastructure";
        var assembly = "PlantBasedPizza.Payment.DataTransfer";
        var implements = typeof(OrderManager.Core.Services.IPaymentService);

        var result = Types.InAssembly(Assembly.Load(assemblyUnderTest))
            .That()
            .ImplementInterface(implements)
            .Should()
            .HaveDependencyOn(assembly)
            .GetResult()
            .IsSuccessful;

        result.Should()
            .BeTrue($"{assemblyUnderTest} is expected to have a dependency on {assembly}");
    }
}