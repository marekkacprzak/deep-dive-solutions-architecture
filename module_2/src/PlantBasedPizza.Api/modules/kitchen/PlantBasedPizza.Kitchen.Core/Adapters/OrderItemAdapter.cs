using System.Text.Json.Serialization;

namespace PlantBasedPizza.Kitchen.Core.Adapters
{
    public record OrderItemAdapter
    {
        [JsonPropertyName("itemName")]
        public string ItemName { get; init; } = "";
        [JsonPropertyName("recipeIdentifier")]
        public string RecipeIdentifier { get; init; } = "";
        [JsonPropertyName("quantity")]
        public int Quantity { get; init; }
    }
}