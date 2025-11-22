using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Logging.Attributes;
using Paramore.Brighter.Policies.Attributes;
using PlantBasedPizza.Delivery.DataTransfer;
using PlantBasedPizza.OrderManager.Core.Handlers;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Policies;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Infrastructure.PublicEventHandlers;

[AsyncApi]
public class DriverCollectedOrderKafkaEventHandler(IServiceScopeFactory serviceScopeFactory, IAmACommandProcessor processor) : RequestHandlerAsync<DriverCollectedOrderEventV1>
{
    [Channel("delivery.driver-collected")] // Creates a Channel
    [SubscribeOperation(typeof(DriverCollectedOrderEvent), Summary = "Handle a driver order collected event.", OperationId = "delivery.driver-collected")]
    [RequestLoggingAsync(step: 1, timing: HandlerTiming.Before)]
    [UseResiliencePipelineAsync(step: 2, policy: Retry.EXPONENTIAL_RETRYPOLICYASYNC)]
    public override async Task<DriverCollectedOrderEventV1> HandleAsync(DriverCollectedOrderEventV1 command, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<DriverCollectedOrderEventHandler>();

        await handler.Handle(new DriverCollectedOrderEvent(command.OrderIdentifier, command.DriverName));
        return await base.HandleAsync(command);
    }

    public override async Task<DriverCollectedOrderEventV1> FallbackAsync(DriverCollectedOrderEventV1 command, CancellationToken cancellationToken = default)
    {
        await processor.PostAsync(new DLQMessage(DriverCollectedOrderEventV1.EventTypeName)
        {
            Data = command.AsString(),
        });
        
        return await base.FallbackAsync(command, cancellationToken);
    }
}