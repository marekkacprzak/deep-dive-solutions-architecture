using Paramore.Brighter;
using PlantBasedPizza.Kitchen.DataTransfer;

namespace PlantBasedPizza.Kitchen.Infrastructure;

public class DistributedEventPublisher(IAmACommandProcessor processor) : KitchenEventPublisher
{
    private List<Id> _ids = new List<Id>();
    
    public async Task AddToEventOutbox(OrderPreparingEventV1 evt)
    {
        _ids.Add(evt.Id);
        await processor.DepositPostAsync(evt);
    }

    public async Task AddToEventOutbox(OrderPrepCompleteEventV1 evt)
    {
        _ids.Add(evt.Id);
        await processor.DepositPostAsync(evt);
    }

    public async Task AddToEventOutbox(OrderBakedEventV1 evt)
    {
        _ids.Add(evt.Id);
        await processor.DepositPostAsync(evt);
    }

    public async Task AddToEventOutbox(OrderQualityCheckedEventV1 evt)
    {
        _ids.Add(evt.Id);
        await processor.DepositPostAsync(evt);
    }

    public async Task ClearOutbox()
    {
        await processor.ClearOutboxAsync(_ids);
        _ids.Clear();
    }
}