using Paramore.Brighter;
using PlantBasedPizza.Kitchen.DataTransfer;

namespace PlantBasedPizza.Kitchen.Infrastructure;

public class DistributedEventPublisher(IAmACommandProcessor processor) : KitchenEventPublisher
{
    public async Task AddToEventOutbox(OrderPreparingEventV1 evt)
    {
        await processor.DepositPostAsync(evt);
    }

    public async Task AddToEventOutbox(OrderPrepCompleteEventV1 evt)
    {
        await processor.DepositPostAsync(evt);
    }

    public async Task AddToEventOutbox(OrderBakedEventV1 evt)
    {
        await processor.DepositPostAsync(evt);
    }

    public async Task AddToEventOutbox(OrderQualityCheckedEventV1 evt)
    {
        await processor.DepositPostAsync(evt);
    }

    public async Task ClearOutbox()
    {
        await processor.ClearOutboxAsync();
    }
}