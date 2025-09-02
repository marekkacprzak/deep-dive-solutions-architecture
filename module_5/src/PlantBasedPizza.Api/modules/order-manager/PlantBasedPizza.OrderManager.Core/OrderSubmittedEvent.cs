using System.Text.Json;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Core
{
    public class OrderSubmittedEvent : DomainEvent
    {
        private readonly string _eventId;

        public OrderSubmittedEvent(string orderIdentifier)
        {
            _eventId = Guid.NewGuid().ToString();
            EventDate = DateTime.Now.ToUniversalTime();
            OrderIdentifier = orderIdentifier;
            CorrelationId = CorrelationContext.GetCorrelationId();
        }

        public string OrderIdentifier { get; private set; }

        public override string EventName => "orders.order-submitted";

        public override string EventVersion => "v1";

        public override string EventId => _eventId;

        public override DateTime EventDate { get; }
        public override string CorrelationId { get; set; }

        public override string AsString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}