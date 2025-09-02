



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
public class OrderBakedKafkaEventHandler(IServiceScopeFactory serviceScopeFactory, IAmACommandProcessor processor) : RequestHandlerAsync<OrderBakedEventV1>
{
    [Channel("kitchen.baked")] // Creates a Channel
    [SubscribeOperation(typeof(OrderBakedEvent), Summary = "Handle an order baked event.", OperationId = "kitchen.baked")]
    [RequestLoggingAsync(step: 1, timing: HandlerTiming.Before)]
    // [UseResiliencePipeline(step: 2, policy: Retry.EXPONENTIAL_RETRYPOLICYASYNC)]
    public override async Task<OrderBakedEventV1> HandleAsync(OrderBakedEventV1 command, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<OrderBakedEventHandler>();

        await handler.Handle(new OrderBakedEvent(command.OrderIdentifier));
        return await base.HandleAsync(command);
    }

    public override async Task<OrderBakedEventV1> FallbackAsync(OrderBakedEventV1 command, CancellationToken cancellationToken = default)
    {
        processor.Post(new DLQMessage(OrderBakedEventV1.EventTypeName)
        {
            Data = command.AsString()
        });
        
        return await base.FallbackAsync(command);
    }
}