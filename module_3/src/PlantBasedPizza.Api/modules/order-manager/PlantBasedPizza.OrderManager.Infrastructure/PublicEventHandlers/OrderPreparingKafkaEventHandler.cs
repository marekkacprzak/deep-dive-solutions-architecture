



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
public class OrderPreparingKafkaEventHandler(IServiceScopeFactory serviceScopeFactory, IAmACommandProcessor processor) : RequestHandlerAsync<OrderPreparingEventV1>
{
    [Channel("kitchen.prep-started")] // Creates a Channel
    [SubscribeOperation(typeof(OrderPreparingEvent), Summary = "Handle an order prep started event.",
        OperationId = "kitchen.prep-started")]
    [RequestLoggingAsync(step: 1, timing: HandlerTiming.Before)]
    // [UseResiliencePipeline(step: 2, policy: Retry.EXPONENTIAL_RETRYPOLICYASYNC)]
    public override async Task<OrderPreparingEventV1> HandleAsync(OrderPreparingEventV1 command, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<OrderPreparingEventHandler>();

        await handler.Handle(new OrderPreparingEvent(command.OrderIdentifier));
        return await base.HandleAsync(command);
    }

    public override async Task<OrderPreparingEventV1> FallbackAsync(OrderPreparingEventV1 command, CancellationToken cancellationToken = default)
    {
        processor.Post(new DLQMessage(OrderPreparingEventV1.EventTypeName)
        {
            Data = command.AsString(),
        });

        return await base.FallbackAsync(command, cancellationToken);
    }
}