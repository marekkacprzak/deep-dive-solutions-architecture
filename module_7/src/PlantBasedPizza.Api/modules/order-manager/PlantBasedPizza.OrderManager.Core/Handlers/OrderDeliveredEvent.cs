namespace PlantBasedPizza.OrderManager.Core.Handlers;

public record OrderDeliveredEvent
{
    public OrderDeliveredEvent(string orderIdentifier)
    {
        OrderIdentifier = orderIdentifier;
    }

    public string OrderIdentifier { get; private set; }
}