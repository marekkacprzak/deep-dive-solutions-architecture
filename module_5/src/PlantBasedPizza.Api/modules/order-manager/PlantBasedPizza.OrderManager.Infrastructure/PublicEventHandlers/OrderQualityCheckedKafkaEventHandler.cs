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
public class OrderQualityCheckedKafkaEventHandler(
    IServiceScopeFactory serviceScopeFactory,
    IAmACommandProcessor processor) : RequestHandlerAsync<OrderQualityCheckedEventV1>
{
    [Channel("kitchen.quality-checked")] // Creates a Channel
    [SubscribeOperation(typeof(OrderQualityCheckedEvent), Summary = "Handle an order quality event.",
        OperationId = "kitchen.quality-checked")]
    [RequestLoggingAsync(1, HandlerTiming.Before)]
    // [UseResiliencePipeline(step: 2, policy: Retry.EXPONENTIAL_RETRYPOLICYASYNC)]
    public override async Task<OrderQualityCheckedEventV1> HandleAsync(OrderQualityCheckedEventV1 command, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<OrderQualityCheckedEventHandler>();

        await handler.Handle(new OrderQualityCheckedEvent(command.OrderIdentifier));
        return await base.HandleAsync(command);
    }

    public override async Task<OrderQualityCheckedEventV1> FallbackAsync(OrderQualityCheckedEventV1 command, CancellationToken cancellationToken = default)
    {
        processor.Post(new DLQMessage(OrderQualityCheckedEventV1.EventTypeName)
        {
            Data = command.AsString()
        });

        return await base.FallbackAsync(command, cancellationToken);
    }
}