



namespace PlantBasedPizza.OrderManager.Core.SubmitOrder;

public record SubmitOrderCommand
{
    public string OrderIdentifier { get; init; }
}