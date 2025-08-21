



using System.Text.Json;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.DataTransfer;

public class OrderCreatedEventV1 : PublicEvent
{
    public static string EventTypeName => "orders.order-created";
    private readonly string _eventId;

    public OrderCreatedEventV1(string orderIdentifier)
    {
        _eventId = Guid.NewGuid().ToString();
        EventDate = DateTime.Now.ToUniversalTime();
        OrderIdentifier = orderIdentifier;
    }

    public string OrderIdentifier { get; private set; }

    public override string EventName => "orders.order-created";

    public override string EventVersion => "v1";

    public override string EventId => _eventId;

    public override DateTime EventDate { get; }

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}