using System.Text.Json;
using Paramore.Brighter;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Events;

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

public class OrderCreatedMessageMapper: IAmAMessageMapper<OrderCreatedEvent> {
    public Message MapToMessage(OrderCreatedEvent request)
    {
        var header = new MessageHeader(messageId: request.Id, topic: request.EventName, messageType: MessageType.MT_EVENT);
        var body = new MessageBody(request.AsString());
        var message = new Message(header, body);
        return message;
    }

    public OrderCreatedEvent MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<OrderCreatedEvent>(message.Body.Value);
    }
}