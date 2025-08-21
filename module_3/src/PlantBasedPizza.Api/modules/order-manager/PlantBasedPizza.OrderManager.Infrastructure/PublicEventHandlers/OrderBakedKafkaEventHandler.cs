



using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using PlantBasedPizza.Kitchen.DataTransfer;
using PlantBasedPizza.OrderManager.Core.Handlers;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Infrastructure.PublicEventHandlers;

[AsyncApi]
public class OrderBakedKafkaEventHandler(IServiceScopeFactory serviceScopeFactory) : RequestHandler<OrderBakedEventV1>
{
    [Channel("kitchen.baked")] // Creates a Channel
    [SubscribeOperation(typeof(OrderBakedEvent), Summary = "Handle an order baked event.", OperationId = "kitchen.baked")]
    public override OrderBakedEventV1 Handle(OrderBakedEventV1 command)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<OrderBakedEventHandler>();

        handler.Handle(new OrderBakedEvent(command.OrderIdentifier)).GetAwaiter().GetResult();
        return base.Handle(command);
    }
}