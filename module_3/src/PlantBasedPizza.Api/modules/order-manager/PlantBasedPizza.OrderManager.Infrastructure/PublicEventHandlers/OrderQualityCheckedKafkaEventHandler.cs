



using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using PlantBasedPizza.Kitchen.DataTransfer;
using PlantBasedPizza.OrderManager.Core.Handlers;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Infrastructure.PublicEventHandlers;

[AsyncApi]
public class OrderQualityCheckedKafkaEventHandler(IServiceScopeFactory serviceScopeFactory) : RequestHandler<OrderQualityCheckedEventV1>
{
    [Channel("kitchen.quality-checked")] // Creates a Channel
    [SubscribeOperation(typeof(OrderQualityCheckedEvent), Summary = "Handle an order quality event.", OperationId = "kitchen.quality-checked")]
    public override OrderQualityCheckedEventV1 Handle(OrderQualityCheckedEventV1 command)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<OrderQualityCheckedEventHandler>();

        handler.Handle(new OrderQualityCheckedEvent(command.OrderIdentifier)).GetAwaiter().GetResult();
        return base.Handle(command);
    }
}