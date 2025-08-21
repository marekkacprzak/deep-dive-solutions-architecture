



using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using PlantBasedPizza.Kitchen.DataTransfer;
using PlantBasedPizza.OrderManager.Core.Handlers;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Infrastructure.PublicEventHandlers;

[AsyncApi]
public class OrderPrepCompleteKafkaEventHandler(IServiceScopeFactory serviceScopeFactory) : RequestHandler<OrderPrepCompleteEventV1>
{
    [Channel("kitchen.prep-complete")] // Creates a Channel
    [SubscribeOperation(typeof(OrderPrepCompleteEvent), Summary = "Handle an order prep completed event.",
        OperationId = "kitchen.prep-complete")]
    public override OrderPrepCompleteEventV1 Handle(OrderPrepCompleteEventV1 command)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<OrderPrepCompleteEventHandler>();

        handler.Handle(new OrderPrepCompleteEvent(command.OrderIdentifier)).GetAwaiter().GetResult();
        return base.Handle(command);
    }
}