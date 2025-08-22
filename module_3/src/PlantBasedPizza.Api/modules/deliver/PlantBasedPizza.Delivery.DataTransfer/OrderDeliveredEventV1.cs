using System.Text.Json;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Delivery.DataTransfer
{
    public class OrderDeliveredEventV1 : PublicEvent
    {
        public static string EventTypeName => "delivery.order-delivered";
        
        public OrderDeliveredEventV1(string orderIdentifier) : base()
        {
            this.OrderIdentifier = orderIdentifier;
        }
        
        
        public override string EventName => EventTypeName;
        
        public override string EventVersion => "v1";

        public string OrderIdentifier { get; private set; }
        public override string AsString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}