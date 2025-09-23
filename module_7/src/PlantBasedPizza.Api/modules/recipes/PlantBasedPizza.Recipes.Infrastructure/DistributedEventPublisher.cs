using Paramore.Brighter;
using PlantBasedPizza.Recipes.DataTransfer;

namespace PlantBasedPizza.Recipes.Infrastructure;

public class DistributedEventPublisher(IAmACommandProcessor processor) : RecipeEventPublisher
{
    private List<Id> _ids = new();
    
    public async Task AddToEventOutbox(RecipeCreatedEventV1 evt)
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