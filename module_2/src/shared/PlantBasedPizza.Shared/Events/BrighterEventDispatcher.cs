using Paramore.Brighter;

namespace PlantBasedPizza.Shared.Events;

public class BrighterEventDispatcher(IAmACommandProcessor processor) : IDomainEventDispatcher
{
    public async Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent, IRequest?
    {
        await processor.PostAsync<IRequest>(domainEvent);
    }
}