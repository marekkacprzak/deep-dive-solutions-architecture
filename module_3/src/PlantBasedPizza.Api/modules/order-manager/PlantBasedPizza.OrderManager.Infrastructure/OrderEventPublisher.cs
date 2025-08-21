



using PlantBasedPizza.OrderManager.DataTransfer;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public interface OrderEventPublisher
{
    Task Publish(OrderCreatedEventV1 evt);
    Task Publish(OrderSubmittedEventV1 evt);
    Task Publish(OrderReadyForDeliveryEventV1 evt);
    Task Publish(OrderCompletedEventV1 evt);
}