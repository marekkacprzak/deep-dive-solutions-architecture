namespace PlantBasedPizza.OrderManager.Core.Handlers;

public record DriverCollectedOrderEvent(string OrderIdentifier, string DriverName)
{
    public string OrderIdentifier { get; private set; } = OrderIdentifier;

    public string DriverName { get; private set; } = DriverName;
}