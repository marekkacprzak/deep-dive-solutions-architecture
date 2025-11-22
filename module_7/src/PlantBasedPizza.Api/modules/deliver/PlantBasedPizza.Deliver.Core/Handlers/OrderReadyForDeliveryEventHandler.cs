using Microsoft.Extensions.Logging;

namespace PlantBasedPizza.Deliver.Core.Handlers
{
    public class OrderReadyForDeliveryEventHandler(
        IDeliveryRequestRepository deliveryRequestRepository,
        ILogger<OrderReadyForDeliveryEventHandler> logger)
    {
        public async Task Handle(OrderReadyForDeliveryEvent evt)
        {
            if (evt == null)
            {
                throw new ArgumentNullException(nameof(evt), "Handled event cannot be null");
            }

            if (evt.OrderIdentifier is null)
            {
                logger.LogWarning("Received ready for delivery event with null OrderIdentifier");
                return;
            }
            
            logger.LogInformation($"Received new ready for delivery event for order {evt.OrderIdentifier}");

            var existingDeliveryRequestForOrder =
                await deliveryRequestRepository.GetDeliveryStatusForOrder(evt.OrderIdentifier);

            if (existingDeliveryRequestForOrder != null)
            {
                logger.LogInformation("Delivery request for order received, skipping");
                return;
            }

            logger.LogInformation("Creating and storing delivery request");

            var request = new DeliveryRequest(evt.OrderIdentifier,
                new Address(evt.DeliveryAddressLine1 ?? string.Empty, evt.DeliveryAddressLine2 ?? string.Empty, evt.DeliveryAddressLine3 ?? string.Empty,
                    evt.DeliveryAddressLine4 ?? string.Empty, evt.DeliveryAddressLine5 ?? string.Empty, evt.Postcode ?? string.Empty));

            await deliveryRequestRepository.AddNewDeliveryRequest(request);

            logger.LogInformation("Delivery request added");
        }
    }
}