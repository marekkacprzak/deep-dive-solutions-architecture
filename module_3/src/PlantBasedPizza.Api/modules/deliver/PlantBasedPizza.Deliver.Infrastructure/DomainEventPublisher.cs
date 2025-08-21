



using PlantBasedPizza.Deliver.Core;
using PlantBasedPizza.Delivery.DataTransfer;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Deliver.Infrastructure;

public class DomainEventPublisher(IDeliveryEventPublisher publisher) : Handles<DriverCollectedOrderEvent>, Handles<OrderDeliveredEvent>
{
    public async Task Handle(DriverCollectedOrderEvent evt)
    {
        await publisher.Publish(new DriverCollectedOrderEventV1(evt.OrderIdentifier, evt.DriverName));
    }

    public async Task Handle(OrderDeliveredEvent evt)
    {
        await publisher.Publish(new OrderDeliveredEventV1(evt.OrderIdentifier));
    }
}