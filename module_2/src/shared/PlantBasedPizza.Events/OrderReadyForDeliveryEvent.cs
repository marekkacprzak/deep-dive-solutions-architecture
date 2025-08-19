using System.Text.Json;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Events
{
    public class OrderReadyForDeliveryEvent : DomainEvent
    {
        public OrderReadyForDeliveryEvent(string orderIdentifier, string addressLine1, string addressLine2, string addressLine3, string addressLine4, string addressLine5, string postcode)
        {
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now.ToUniversalTime();
            this.OrderIdentifier = orderIdentifier;
            this.DeliveryAddressLine1 = addressLine1;
            this.DeliveryAddressLine2 = addressLine2;
            this.DeliveryAddressLine3 = addressLine3;
            this.DeliveryAddressLine4 = addressLine4;
            this.DeliveryAddressLine5 = addressLine5;
            this.Postcode = postcode;
            this.CorrelationId = CorrelationContext.GetCorrelationId();
        }
        
        public override string EventName => "order-manager.ready-for-delivery";
        public override string EventVersion => "v1";

        public override string EventId { get; }
        
        public override DateTime EventDate { get; }
        public override string CorrelationId { get; set; }

        public string OrderIdentifier { get; private set; }
        
        public string DeliveryAddressLine1 { get; private set; }
        
        public string DeliveryAddressLine2 { get; private set; }
        
        public string DeliveryAddressLine3 { get; private set; }
        
        public string DeliveryAddressLine4 { get; private set; }
        
        public string DeliveryAddressLine5 { get; private set; }
        
        public string Postcode { get; private set; }

        public override string AsString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}