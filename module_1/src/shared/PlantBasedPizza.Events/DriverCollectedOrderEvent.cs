using System.Text.Json;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Events
{
    public class DriverCollectedOrderEvent : DomainEvent
    {
        public DriverCollectedOrderEvent(string orderIdentifier, string driverName)
        {
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now.ToUniversalTime();
            this.OrderIdentifier = orderIdentifier;
            this.DriverName = driverName;
            this.CorrelationId = CorrelationContext.GetCorrelationId();
        }
        
        public override string EventName => "delivery.driver-collected";
        
        public override string EventVersion => "v1";
        
        public override string EventId { get; }
        
        public override DateTime EventDate { get; }
        public override string CorrelationId { get; set; }

        public string OrderIdentifier { get; private set; }
        
        public string DriverName { get; private set; }
        public override string AsString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}