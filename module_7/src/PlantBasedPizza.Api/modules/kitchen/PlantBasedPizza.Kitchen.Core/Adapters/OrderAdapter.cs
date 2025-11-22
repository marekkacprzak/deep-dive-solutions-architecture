using System.Text.Json.Serialization;

namespace PlantBasedPizza.Kitchen.Core.Adapters
{
    public record OrderAdapter
    {
        [JsonPropertyName("items")]
        public List<OrderItemAdapter>? Items { get; init; }
    }
}