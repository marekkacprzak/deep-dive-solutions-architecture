using System.Text.Json.Serialization;
using Paramore.Brighter;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PlantBasedPizza.Events
{
    public class OrderCompletedEvent : DomainEvent
    {
        private readonly string _eventId;

        public OrderCompletedEvent(string customerIdentifier, string orderIdentifier, decimal orderValue)
        {
            this._eventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now.ToUniversalTime();
            this.CustomerIdentifier = customerIdentifier;
            this.OrderIdentifier = orderIdentifier;
            OrderValue = orderValue;
            this.CorrelationId = CorrelationContext.GetCorrelationId();
        }
        
        [JsonPropertyName("customerIdentifier")]
        public string CustomerIdentifier { get; set; }

        [JsonPropertyName("orderIdentifier")]
        public string OrderIdentifier { get; set; }
        
        [JsonPropertyName("orderValue")]
        public decimal OrderValue { get; set; }
        
        public override string EventName => "orders.order-completed";
        public override string EventVersion => "v1";
        public override string AsString()
        {
            return JsonSerializer.Serialize(this);
        }

        public override string EventId => this._eventId;

        public override DateTime EventDate { get; }
        public override string CorrelationId { get; set; }
    }
}

public class OrderCompletedMessageMapper: IAmAMessageMapper<OrderCompletedEvent> {
    public Message MapToMessage(OrderCompletedEvent request)
    {
        var header = new MessageHeader(messageId: request.Id, topic: request.EventName, messageType: MessageType.MT_EVENT);
        var body = new MessageBody(request.AsString());
        var message = new Message(header, body);
        return message;
    }

    public OrderCompletedEvent MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<OrderCompletedEvent>(message.Body.Value);
    }
}