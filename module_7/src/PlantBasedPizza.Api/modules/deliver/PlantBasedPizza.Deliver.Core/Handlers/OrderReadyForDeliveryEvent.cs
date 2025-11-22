using System.Text.Json.Serialization;

namespace PlantBasedPizza.Deliver.Core.Handlers;

public record OrderReadyForDeliveryEvent
{
    [JsonConstructor]
    public OrderReadyForDeliveryEvent()
    {
    }

    public OrderReadyForDeliveryEvent(string? orderIdentifier, string? addressLine1, string? addressLine2,
        string? addressLine3, string? addressLine4, string? addressLine5, string? postcode)
    {
        OrderIdentifier = orderIdentifier;
        DeliveryAddressLine1 = addressLine1;
        DeliveryAddressLine2 = addressLine2;
        DeliveryAddressLine3 = addressLine3;
        DeliveryAddressLine4 = addressLine4;
        DeliveryAddressLine5 = addressLine5;
        Postcode = postcode;
    }

    public string? OrderIdentifier { get; init; }

    public string? DeliveryAddressLine1 { get; init; }

    public string? DeliveryAddressLine2 { get; init; }

    public string? DeliveryAddressLine3 { get; init; }

    public string? DeliveryAddressLine4 { get; init; }

    public string? DeliveryAddressLine5 { get; init; }

    public string? Postcode { get; init; }
}