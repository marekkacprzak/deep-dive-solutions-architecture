using PlantBasedPizza.Delivery.DataTransfer;

namespace PlantBasedPizza.Deliver.Infrastructure;

public interface IDeliveryEventPublisher
{
    Task Publish(DriverCollectedOrderEventV1 evt);
    Task Publish(OrderDeliveredEventV1 evt);
}