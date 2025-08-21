



using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using PlantBasedPizza.Kitchen.DataTransfer;
using PlantBasedPizza.OrderManager.Core.Handlers;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Infrastructure.PublicEventHandlers;

[AsyncApi]
public class OrderPreparingKafkaEventHandler(IServiceScopeFactory serviceScopeFactory) : RequestHandler<OrderPreparingEventV1>
{
    [Channel("kitchen.prep-started")] // Creates a Channel
    [SubscribeOperation(typeof(OrderPreparingEvent), Summary = "Handle an order prep started event.",
        OperationId = "kitchen.prep-started")]
    public override OrderPreparingEventV1 Handle(OrderPreparingEventV1 command)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<OrderPreparingEventHandler>();

        handler.Handle(new OrderPreparingEvent(command.OrderIdentifier)).GetAwaiter().GetResult();
        return base.Handle(command);
    }
}