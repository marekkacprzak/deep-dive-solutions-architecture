using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;
using Paramore.Brighter.Logging.Attributes;
using Paramore.Brighter.Policies.Attributes;
using PlantBasedPizza.Kitchen.Core.Handlers;
using PlantBasedPizza.OrderManager.DataTransfer;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Policies;
using Saunter.Attributes;
using OrderSubmittedEvent = PlantBasedPizza.Kitchen.Core.Handlers.OrderSubmittedEvent;

namespace PlantBasedPizza.Kitchen.Infrastructure.EventHandlers;

[AsyncApi]
public class OrderSubmittedKafkaEventHandler(
    IServiceScopeFactory serviceScopeFactory,
    IAmACommandProcessor processor,
    ILogger<OrderSubmittedKafkaEventHandler> logger)
    : RequestHandlerAsync<OrderSubmittedEventV1>
{
    [Channel("order-manager.order-submitted")] // Creates a Channel
    [SubscribeOperation(typeof(OrderSubmittedEvent), Summary = "Handle an order submitted event.",
        OperationId = "order-manager.order-submitted")]
    [RequestLoggingAsync(1, HandlerTiming.Before)]
    // [UseResiliencePipeline(step: 2, policy: Retry.EXPONENTIAL_RETRYPOLICYASYNC)]
    public override async Task<OrderSubmittedEventV1> HandleAsync(OrderSubmittedEventV1 command,
        CancellationToken cancellationToken = new())
    {
        try
        {
            logger.LogInformation("Handling OrderSubmittedEvent for OrderId: {OrderId}", command.OrderIdentifier);

            using var scope = serviceScopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<OrderSubmittedEventHandler>();

            await handler.Handle(new OrderSubmittedEvent(command.OrderIdentifier));
        }
        // Generic exception handling to ensure all errors get routed to the DLQ
        catch (Exception ex)
        {
            await FallbackAsync(command);
        }

        return await base.HandleAsync(command);
    }

    public override async Task<OrderSubmittedEventV1> FallbackAsync(OrderSubmittedEventV1 command,
        CancellationToken cancellationToken = new())
    {
        processor.Post(new DLQMessage(OrderSubmittedEventV1.EventTypeName)
        {
            Data = command.AsString()
        });

        return await base.FallbackAsync(command, cancellationToken);
    }
}