using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;
using Paramore.Brighter.Logging.Attributes;
using PlantBasedPizza.Kitchen.Core.Handlers;
using PlantBasedPizza.OrderManager.DataTransfer;
using Saunter.Attributes;
using OrderSubmittedEvent = PlantBasedPizza.Kitchen.Core.Handlers.OrderSubmittedEvent;

namespace PlantBasedPizza.Kitchen.Infrastructure.EventHandlers;

[AsyncApi]
public class OrderSubmittedKafkaEventHandler(
    IServiceScopeFactory serviceScopeFactory,
    IAmACommandProcessor processor,
    ILogger<OrderSubmittedKafkaEventHandler> logger)
    : RequestHandler<OrderSubmittedEventV1>
{
    [Channel("order-manager.order-submitted")] // Creates a Channel
    [SubscribeOperation(typeof(OrderSubmittedEvent), Summary = "Handle an order submitted event.", OperationId = "order-manager.order-submitted")]
    [RequestLogging(step: 1, timing: HandlerTiming.Before)]
    public override OrderSubmittedEventV1 Handle(OrderSubmittedEventV1 command)
    {
        try
        {
            logger.LogInformation("Handling OrderSubmittedEvent for OrderId: {OrderId}", command.OrderIdentifier);

            using var scope = serviceScopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<OrderSubmittedEventHandler>();

            handler.Handle(new OrderSubmittedEvent(command.OrderIdentifier)).GetAwaiter().GetResult();
        }
        // Generic exception handling to ensure all errors get routed to the DLQ
        catch (Exception ex)
        {
            Fallback(command);
        }
        
        return base.Handle(command);
    }

    public override OrderSubmittedEventV1 Fallback(OrderSubmittedEventV1 command)
    {
        processor.Post(new DLQMessage()
        {
            Data = command.AsString(),
            EventName = OrderSubmittedEventV1.EventTypeName
        });
        
        return base.Handle(command);
    }
}