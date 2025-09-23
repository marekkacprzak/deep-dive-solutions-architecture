



using System.Text.Json;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.DataTransfer;

public class OrderCompletedEventV1 : PublicEvent
{
    public static string EventTypeName => "orders.order-completed";

    public OrderCompletedEventV1(string orderIdentifier) : base()
    {
        OrderIdentifier = orderIdentifier;
    }

    public string OrderIdentifier { get; private set; }

    public override string EventName => "orders.order-completed";

    public override string EventVersion => "v1";

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}