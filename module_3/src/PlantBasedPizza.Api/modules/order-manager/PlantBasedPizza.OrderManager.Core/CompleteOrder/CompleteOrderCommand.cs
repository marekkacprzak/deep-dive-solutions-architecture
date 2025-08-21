



namespace PlantBasedPizza.OrderManager.Core.CompleteOrder;

public record CompleteOrderCommand
{
    public string OrderIdentifier { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
}