



using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using PlantBasedPizza.Delivery.DataTransfer;
using PlantBasedPizza.OrderManager.Core.Handlers;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Infrastructure.PublicEventHandlers;

[AsyncApi]
public class OrderDeliveredKafkaEventHandler(IServiceScopeFactory serviceScopeFactory) : RequestHandler<OrderDeliveredEventV1>
{
    [Channel("delivery.order-delivered")] // Creates a Channel
    [SubscribeOperation(typeof(OrderDeliveredEvent), Summary = "Handle an order delivered event.",
        OperationId = "delivery.order-delivered")]
    public override OrderDeliveredEventV1 Handle(OrderDeliveredEventV1 command)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<DriverDeliveredOrderEventHandler>();

        handler.Handle(new OrderDeliveredEvent(command.OrderIdentifier)).GetAwaiter().GetResult();
        return base.Handle(command);
    }
}