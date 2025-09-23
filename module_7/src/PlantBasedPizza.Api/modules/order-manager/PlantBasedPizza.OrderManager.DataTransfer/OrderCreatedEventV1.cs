



using System.Text.Json;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.DataTransfer;

public class OrderCreatedEventV1 : PublicEvent
{
    public static string EventTypeName => "orders.order-created";

    public OrderCreatedEventV1(string orderIdentifier) : base()
    {
        OrderIdentifier = orderIdentifier;
    }

    public string OrderIdentifier { get; private set; }

    public override string EventName => "orders.order-created";

    public override string EventVersion => "v1";

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}