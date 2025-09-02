using PlantBasedPizza.OrderManager.DataTransfer;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class NoOpEventPublisher : OrderEventPublisher
{
    public Task Publish(OrderCreatedEventV1 evt)
    {
        //NoOp
        return Task.CompletedTask;
    }

    public Task Publish(OrderSubmittedEventV1 evt)
    {
        //NoOp
        return Task.CompletedTask;
    }

    public Task Publish(OrderReadyForDeliveryEventV1 evt)
    {
        return Task.CompletedTask;
    }

    public Task Publish(OrderCompletedEventV1 evt)
    {
        //NoOp
        return Task.CompletedTask;
    }
}