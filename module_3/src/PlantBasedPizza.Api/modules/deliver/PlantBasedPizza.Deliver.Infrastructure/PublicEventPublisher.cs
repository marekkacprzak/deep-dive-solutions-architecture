



using Paramore.Brighter;
using PlantBasedPizza.Delivery.DataTransfer;

namespace PlantBasedPizza.Deliver.Infrastructure;

public class PublicEventPublisher(IAmACommandProcessor processor) : IDeliveryEventPublisher
{
    public async Task Publish(DriverCollectedOrderEventV1 evt)
    {
        await processor.PostAsync(evt);
    }

    public async Task Publish(OrderDeliveredEventV1 evt)
    {
        await processor.PostAsync(evt);
    }
}