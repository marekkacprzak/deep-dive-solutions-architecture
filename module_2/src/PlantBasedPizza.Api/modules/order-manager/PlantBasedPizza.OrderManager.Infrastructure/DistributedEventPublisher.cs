using Paramore.Brighter;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class DistributedEventPublisher(IAmACommandProcessor processor) : OrderEventPublisher
{
    public async Task Publish(OrderCreatedEvent evt)
    {
        await processor.PostAsync(evt);
    }

    public async Task Publish(OrderSubmittedEvent evt)
    {
        await processor.PostAsync(evt);
    }

    public async Task Publish(OrderCompletedEvent evt)
    {
        await processor.PostAsync(evt);
    }
}