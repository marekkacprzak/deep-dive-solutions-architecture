using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class NoOpEventPublisher : OrderEventPublisher
{
    public Task Publish(OrderCreatedEvent evt)
    {
        //NoOp
        return Task.CompletedTask;
    }

    public Task Publish(OrderSubmittedEvent evt)
    {
        //NoOp
        return Task.CompletedTask;
    }

    public Task Publish(OrderCompletedEvent evt)
    {
        //NoOp
        return Task.CompletedTask;
    }
}