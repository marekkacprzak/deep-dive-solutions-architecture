



using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using PlantBasedPizza.Delivery.DataTransfer;
using PlantBasedPizza.OrderManager.Core.Handlers;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Infrastructure.PublicEventHandlers;

[AsyncApi]
public class DriverCollectedOrderKafkaEventHandler(IServiceScopeFactory serviceScopeFactory) : RequestHandler<DriverCollectedOrderEventV1>
{
    [Channel("delivery.driver-collected")] // Creates a Channel
    [SubscribeOperation(typeof(DriverCollectedOrderEvent), Summary = "Handle a driver order collected event.", OperationId = "delivery.driver-collected")]
    public override DriverCollectedOrderEventV1 Handle(DriverCollectedOrderEventV1 command)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<DriverCollectedOrderEventHandler>();

        handler.Handle(new DriverCollectedOrderEvent(command.OrderIdentifier, command.DriverName)).GetAwaiter().GetResult();
        return base.Handle(command);
    }
}