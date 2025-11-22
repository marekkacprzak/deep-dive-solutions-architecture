



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
public class OrderDeliveredKafkaEventHandler(IServiceScopeFactory serviceScopeFactory, IAmACommandProcessor processor) : RequestHandlerAsync<OrderDeliveredEventV1>
{
    [Channel("delivery.order-delivered")] // Creates a Channel
    [SubscribeOperation(typeof(OrderDeliveredEvent), Summary = "Handle an order delivered event.",
        OperationId = "delivery.order-delivered")]
    [RequestLoggingAsync(step: 1, timing: HandlerTiming.Before)]
    [UseResiliencePipelineAsync(step: 2, policy: Retry.EXPONENTIAL_RETRYPOLICYASYNC)]
    public override async Task<OrderDeliveredEventV1> HandleAsync(OrderDeliveredEventV1 command, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<DriverDeliveredOrderEventHandler>();

        await handler.Handle(new OrderDeliveredEvent(command.OrderIdentifier));
        return await base.HandleAsync(command);
    }

    public override async Task<OrderDeliveredEventV1> FallbackAsync(OrderDeliveredEventV1 command, CancellationToken cancellationToken = default)
    {
        processor.Post(new DLQMessage(OrderDeliveredEventV1.EventTypeName)
        {
            Data = command.AsString(),
        });
        
        return await base.FallbackAsync(command, cancellationToken);
    }
}