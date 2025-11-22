using System.Diagnostics;

namespace PlantBasedPizza.Deliver.Core.GetDelivery;

public class GetDeliveryQueryHandler
{
    private readonly IDeliveryRequestRepository _deliveryRequestRepository;

    public GetDeliveryQueryHandler(IDeliveryRequestRepository deliveryRequestRepository)
    {
        _deliveryRequestRepository = deliveryRequestRepository;
    }

    public async Task<DeliveryRequestDTO> Handle(GetDeliveryQuery query)
    {
        Activity.Current?.AddTag("orderIdentifier", query.OrderIdentifier);
            
        var deliveryRequest = await this._deliveryRequestRepository.GetDeliveryStatusForOrder(query.OrderIdentifier);

        if (deliveryRequest is null)
        {
            throw new ArgumentException($"Delivery request not found for order {query.OrderIdentifier}");
        }

        return new DeliveryRequestDTO(deliveryRequest);
    }
}