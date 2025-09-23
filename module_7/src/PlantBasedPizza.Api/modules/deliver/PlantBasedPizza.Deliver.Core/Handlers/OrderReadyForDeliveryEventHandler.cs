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
                new Address(evt.DeliveryAddressLine1, evt.DeliveryAddressLine2, evt.DeliveryAddressLine3,
                    evt.DeliveryAddressLine4, evt.DeliveryAddressLine5, evt.Postcode));

            await deliveryRequestRepository.AddNewDeliveryRequest(request);

            logger.LogInformation("Delivery request added");
        }
    }
}