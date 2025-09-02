using System.Text.Json;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Recipes.DataTransfer;

public class RecipeCreatedEventV1 : PublicEvent
{
    public static string EventTypeName => "recipe.recipe-created";

    public RecipeCreatedEventV1(string recipeIdentifier, string name, decimal price) : base()
    {
        RecipeIdentifier = recipeIdentifier;
        Name = name;
        Price = price;
    }

    public string RecipeIdentifier { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    public override string EventName => EventTypeName;

    public override string EventVersion => "v1";

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}