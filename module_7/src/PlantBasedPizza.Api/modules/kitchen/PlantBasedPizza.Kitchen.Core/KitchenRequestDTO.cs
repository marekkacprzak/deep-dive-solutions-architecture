



using System.Text.Json.Serialization;
using PlantBasedPizza.Kitchen.Core.Adapters;

namespace PlantBasedPizza.Kitchen.Core;

public record KitchenRequestDTO
{
    public KitchenRequestDTO(KitchenRequest request)
    {
        OrderIdentifier = request.OrderIdentifier;
        KitchenRequestId = request.KitchenRequestId;
        OrderReceivedOn = request.OrderReceivedOn;
        OrderState = request.OrderState;
        PrepCompleteOn = request.PrepCompleteOn;
        BakeCompleteOn = request.BakeCompleteOn;
        QualityCheckCompleteOn = request.QualityCheckCompleteOn;
        Recipes = request.Recipes;
    }

    [JsonPropertyName("kitchenRequestId")] public string KitchenRequestId { get; set; } = "";

    [JsonPropertyName("orderIdentifier")] public string OrderIdentifier { get; set; } = "";

    [JsonPropertyName("orderReceivedOn")] public DateTime OrderReceivedOn { get; set; }

    [JsonPropertyName("orderState")] public OrderState OrderState { get; set; }

    [JsonPropertyName("prepCompleteOn")] public DateTime? PrepCompleteOn { get; set; }

    [JsonPropertyName("bakeCompleteOn")] public DateTime? BakeCompleteOn { get; set; }

    [JsonPropertyName("qualityCheckCompleteOn")]
    public DateTime? QualityCheckCompleteOn { get; set; }

    [JsonPropertyName("recipes")] public List<RecipeAdapter> Recipes { get; set; }
}