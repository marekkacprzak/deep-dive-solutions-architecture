



using System.Text.Json;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.DataTransfer;

public class OrderSubmittedEventV1 : PublicEvent
{
    public static string EventTypeName => "orders.order-submitted";

    public OrderSubmittedEventV1(string orderIdentifier) : base()
    {
        OrderIdentifier = orderIdentifier;
    }

    public string OrderIdentifier { get; private set; }

    public override string EventName => EventTypeName;

    public override string EventVersion => "v1";

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}