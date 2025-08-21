



using PlantBasedPizza.Kitchen.DataTransfer;

namespace PlantBasedPizza.Kitchen.Infrastructure;

public interface KitchenEventPublisher
{
    Task AddToEventOutbox(OrderPreparingEventV1 evt);
    Task AddToEventOutbox(OrderPrepCompleteEventV1 evt);
    Task AddToEventOutbox(OrderBakedEventV1 evt);
    Task AddToEventOutbox(OrderQualityCheckedEventV1 evt);

    Task ClearOutbox();
}