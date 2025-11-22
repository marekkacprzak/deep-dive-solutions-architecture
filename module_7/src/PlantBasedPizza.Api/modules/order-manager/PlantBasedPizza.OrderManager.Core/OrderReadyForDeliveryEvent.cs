using System.Text.Json;
using System.Text.Json.Serialization;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Core
{
    public class OrderReadyForDeliveryEvent : DomainEvent
    {
        [JsonConstructor]
        public OrderReadyForDeliveryEvent()
        {
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now.ToUniversalTime();
            this.CorrelationId = CorrelationContext.GetCorrelationId();
        }
        
        public OrderReadyForDeliveryEvent(string? orderIdentifier, string? addressLine1, string? addressLine2, string? addressLine3, string? addressLine4, string? addressLine5, string? postcode)
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

        public string? OrderIdentifier { get; init; }
        
        public string? DeliveryAddressLine1 { get; init; }
        
        public string? DeliveryAddressLine2 { get; init; }
        
        public string? DeliveryAddressLine3 { get; init; }
        
        public string? DeliveryAddressLine4 { get; init; }
        
        public string? DeliveryAddressLine5 { get; init; }
        
        public string? Postcode { get; init; }

        public override string AsString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}