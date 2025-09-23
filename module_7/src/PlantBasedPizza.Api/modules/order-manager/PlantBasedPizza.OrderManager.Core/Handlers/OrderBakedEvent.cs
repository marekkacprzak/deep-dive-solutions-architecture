namespace PlantBasedPizza.OrderManager.Core.Handlers;

public record OrderBakedEvent
{
    public OrderBakedEvent(string orderIdentifier)
    {
        OrderIdentifier = orderIdentifier;
    }

    public string OrderIdentifier { get; private set; }
}