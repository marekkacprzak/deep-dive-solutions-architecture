using System.Text.Json;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Events
{
    public class OrderPreparingEvent : DomainEvent
    {
        public OrderPreparingEvent(string orderIdentifier)
        {
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now.ToUniversalTime();
            this.OrderIdentifier = orderIdentifier;
            this.CorrelationId = CorrelationContext.GetCorrelationId();
        }
        
        public override string EventName => "kitchen.prep-started";
        
        public override string EventVersion => "v1";
        
        public override string EventId { get; }
        
        public override DateTime EventDate { get; }
        public override string CorrelationId { get; set; }

        public string OrderIdentifier { get; private set; }
        public override string AsString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}