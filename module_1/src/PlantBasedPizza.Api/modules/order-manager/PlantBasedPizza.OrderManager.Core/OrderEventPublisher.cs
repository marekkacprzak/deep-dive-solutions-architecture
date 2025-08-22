



using PlantBasedPizza.Events;

namespace PlantBasedPizza.OrderManager.Core;

public interface OrderEventPublisher
{
    Task Publish(OrderCompletedEvent evt);
}