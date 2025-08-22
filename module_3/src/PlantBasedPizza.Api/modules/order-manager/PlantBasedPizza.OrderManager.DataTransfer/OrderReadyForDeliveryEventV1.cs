



using System.Text.Json;
using System.Text.Json.Serialization;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.DataTransfer;

public class OrderReadyForDeliveryEventV1 : PublicEvent
{
    public static string EventTypeName => "orders.order-ready-for-delivery";

    [JsonConstructor]
    public OrderReadyForDeliveryEventV1() : base()
    {
    }

    public OrderReadyForDeliveryEventV1(string orderIdentifier, string addressLine1, string addressLine2,
        string addressLine3, string addressLine4, string addressLine5, string postcode) : base()
    {
        OrderIdentifier = orderIdentifier;
        DeliveryAddressLine1 = addressLine1;
        DeliveryAddressLine2 = addressLine2;
        DeliveryAddressLine3 = addressLine3;
        DeliveryAddressLine4 = addressLine4;
        DeliveryAddressLine5 = addressLine5;
        Postcode = postcode;
    }

    public string OrderIdentifier { get; init; }

    public string DeliveryAddressLine1 { get; init; }

    public string DeliveryAddressLine2 { get; init; }

    public string DeliveryAddressLine3 { get; init; }

    public string DeliveryAddressLine4 { get; init; }

    public string DeliveryAddressLine5 { get; init; }

    public string Postcode { get; init; }

    public override string EventName => EventTypeName;

    public override string EventVersion => "v1";

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}