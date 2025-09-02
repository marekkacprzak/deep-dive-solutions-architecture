namespace PlantBasedPizza.OrderManager.Core.Handlers;

public record OrderQualityCheckedEvent
{
    public OrderQualityCheckedEvent(string orderIdentifier)
    {
        OrderIdentifier = orderIdentifier;
    }

    public string OrderIdentifier { get; private set; }
}