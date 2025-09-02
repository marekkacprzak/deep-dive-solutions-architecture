using System.Text.Json;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Core;

public class OrderCreatedEvent : DomainEvent
{
    private readonly string _eventId;

    public OrderCreatedEvent(string orderIdentifier)
    {
        this._eventId = Guid.NewGuid().ToString();
        this.EventDate = DateTime.Now.ToUniversalTime();
        this.OrderIdentifier = orderIdentifier;
        this.CorrelationId = CorrelationContext.GetCorrelationId();
    }

    public string OrderIdentifier { get; private set; }

    public override string EventName => "orders.order-created";

    public override string EventVersion => "v1";

    public override string EventId => this._eventId;

    public override DateTime EventDate { get; }
    public override string CorrelationId { get; set; }
    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}