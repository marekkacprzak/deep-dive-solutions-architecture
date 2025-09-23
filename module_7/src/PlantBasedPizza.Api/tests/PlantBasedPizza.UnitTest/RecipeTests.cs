using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Recipes.Core.Events;
using PlantBasedPizza.Recipes.Core.Services;
using PlantBasedPizza.Shared.Events;
using Xunit;

namespace PlantBasedPizza.UnitTest;

public class RecipeTests
{
    internal const string DefaultRecipeIdentifier = "MyRecipe";

    [Fact]
    public void DeserializeTest()
    {
        var input =
            "[ {\n  \"recipeIdentifier\" : \"CREATEORDERTEST\",\n  \"name\" : \"CREATEORDERTEST\",\n  \"price\" : 10,\n  \"ingredients\" : [ {\n    \"IngredientIdentifier\" : 1,\n    \"RecipeIdentifier\" : \"CREATEORDERTEST\",\n    \"Name\" : \"Pizza\",\n    \"Quantity\" : 1\n  } ]\n}, {\n  \"recipeIdentifier\" : \"marg\",\n  \"name\" : \"Margharita\",\n  \"price\" : 3.99,\n  \"ingredients\" : [ {\n    \"IngredientIdentifier\" : 2,\n    \"RecipeIdentifier\" : \"marg\",\n    \"Name\" : \"Cheese\",\n    \"Quantity\" : 10\n  }, {\n    \"IngredientIdentifier\" : 3,\n    \"RecipeIdentifier\" : \"marg\",\n    \"Name\" : \"Tomato\",\n    \"Quantity\" : 20\n  } ]\n} ]";
        
        var recipeList = System.Text.Json.JsonSerializer.Deserialize<List<Recipe>>(input);
        
        recipeList.Should().NotBeNull();
        recipeList.Should().HaveCount(2);
        recipeList[0].RecipeIdentifier.Should().Be("CREATEORDERTEST");
        recipeList[0].Ingredients.Should().HaveCount(1);
        recipeList[0].Ingredients[0].Name.Should().Be("Pizza");

    }
    
    [Fact]
    public async Task CanCreateNewRecipe_ShouldSetDefaultFields()
    {
        // Arrange
        var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
        var recipeFactory = new RecipeFactory(mockEventDispatcher);

        // Act
        var recipe = await recipeFactory.CreateAsync(DefaultRecipeIdentifier, "Pizza", 6.5M);
        
        recipe.AddIngredient("Base", 1);
        recipe.AddIngredient("Tomato Sauce", 1);
        recipe.AddIngredient("Cheese", 1);

        // Assert
        recipe.RecipeIdentifier.Should().Be(DefaultRecipeIdentifier);
        recipe.Name.Should().Be("Pizza");
        recipe.Price.Should().Be(6.5M);
        recipe.Ingredients.Count.Should().Be(3);

        A.CallTo(() => mockEventDispatcher.PublishAsync(A<RecipeCreatedEvent>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}