using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Logging.Attributes;
using Paramore.Brighter.Policies.Attributes;
using PlantBasedPizza.Kitchen.DataTransfer;
using PlantBasedPizza.OrderManager.Core.Handlers;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Policies;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Infrastructure.PublicEventHandlers;

[AsyncApi]
public class OrderPrepCompleteKafkaEventHandler(IServiceScopeFactory serviceScopeFactory, IAmACommandProcessor processor) : RequestHandlerAsync<OrderPrepCompleteEventV1>
{
    [Channel("kitchen.prep-complete")] // Creates a Channel
    [SubscribeOperation(typeof(OrderPrepCompleteEventV1), Summary = "Handle an order prep completed event.",
        OperationId = "kitchen.prep-complete")]
    [RequestLoggingAsync(step: 1, timing: HandlerTiming.Before)]
    // [UseResiliencePipeline(step: 2, policy: Retry.EXPONENTIAL_RETRYPOLICYASYNC)]
    public override async Task<OrderPrepCompleteEventV1> HandleAsync(OrderPrepCompleteEventV1 command, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<OrderPrepCompleteEventHandler>();

        await handler.Handle(new OrderPrepCompleteEvent(command.OrderIdentifier));
        return await base.HandleAsync(command);
    }

    public override async Task<OrderPrepCompleteEventV1> FallbackAsync(OrderPrepCompleteEventV1 command, CancellationToken cancellationToken = default)
    {
        processor.Post(new DLQMessage(OrderPrepCompleteEventV1.EventTypeName)
        {
            Data = command.AsString(),
        });
        
        return await base.FallbackAsync(command, cancellationToken);
    }
}