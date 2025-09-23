using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core;

public record OrderDto
{
    public OrderDto(Order order)
    {
        OrderIdentifier = order.OrderIdentifier;
        OrderNumber = order.OrderNumber;
        OrderDate = order.OrderDate;
        AwaitingCollection = order.AwaitingCollection;
        OrderSubmittedOn = order.OrderSubmittedOn;
        OrderCompletedOn = order.OrderCompletedOn;
        Items = order.Items.Select(item => new OrderItemDto()
        {
            ItemName = item.ItemName,
            Price = item.Price,
            Quantity = item.Quantity,
            RecipeIdentifier = item.RecipeIdentifier
        }).ToList();
        History = order.History.Select(history => new OrderHistoryDto()
        {
            Description = history.Description,
            HistoryDate = history.HistoryDate
        }).ToList();
    }
    
    [JsonPropertyName("orderIdentifier")]
    public string OrderIdentifier { get; set; }
    
    [JsonPropertyName("orderNumber")]
    public string OrderNumber { get; set; }
    
    [JsonPropertyName("orderDate")]
    public DateTime OrderDate { get; set; }
    
    [JsonPropertyName("awaitingCollection")]
    public bool AwaitingCollection { get; set; }
    
    [JsonPropertyName("orderSubmittedOn")]
    public DateTime? OrderSubmittedOn { get; set; }
    
    [JsonPropertyName("orderCompletedOn")]
    public DateTime? OrderCompletedOn { get; set; }
    
    [JsonPropertyName("items")]
    public List<OrderItemDto> Items { get; set; }
        
    [JsonPropertyName("history")]
    public List<OrderHistoryDto> History { get; set; }
}