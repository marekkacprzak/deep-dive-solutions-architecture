using System.Text.Json;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Delivery.DataTransfer
{
    public class OrderDeliveredEventV1 : PublicEvent
    {
        public static string EventTypeName => "delivery.order-delivered";
        
        public OrderDeliveredEventV1(string orderIdentifier)
        {
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now.ToUniversalTime();
            this.OrderIdentifier = orderIdentifier;
        }
        
        
        public override string EventName => EventTypeName;
        
        public override string EventVersion => "v1";
        
        public override string EventId { get; }
        
        public override DateTime EventDate { get; }

        public string OrderIdentifier { get; private set; }
        public override string AsString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}