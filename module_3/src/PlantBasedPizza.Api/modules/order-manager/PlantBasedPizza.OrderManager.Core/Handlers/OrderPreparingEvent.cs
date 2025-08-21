namespace PlantBasedPizza.OrderManager.Core.Handlers;

public record OrderPreparingEvent
{
    public OrderPreparingEvent(string orderIdentifier)
    {
        OrderIdentifier = orderIdentifier;
    }

    public string OrderIdentifier { get; private set; }
}