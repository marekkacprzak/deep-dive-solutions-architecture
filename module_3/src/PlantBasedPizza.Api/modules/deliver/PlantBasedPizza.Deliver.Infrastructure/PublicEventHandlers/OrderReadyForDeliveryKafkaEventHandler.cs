



using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.OrderManager.DataTransfer;
using Saunter.Attributes;

namespace PlantBasedPizza.Deliver.Infrastructure.PublicEventHandlers;

[AsyncApi]
public class OrderReadyForDeliveryKafkaEventHandler(IServiceScopeFactory serviceScopeFactory)
    : RequestHandler<OrderReadyForDeliveryEventV1>
{
    [Channel("order-manager.ready-for-delivery")] // Creates a Channel
    [SubscribeOperation(typeof(OrderReadyForDeliveryEvent), Summary = "Handle an order ready for delivery event.", OperationId = "order-manager.ready-for-delivery")]
    public override OrderReadyForDeliveryEventV1 Handle(OrderReadyForDeliveryEventV1 command)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<OrderReadyForDeliveryEventHandler>();

        handler.Handle(new OrderReadyForDeliveryEvent(
            command.OrderIdentifier,
            command.DeliveryAddressLine1,
            command.DeliveryAddressLine2,
            command.DeliveryAddressLine3,
            command.DeliveryAddressLine4,
            command.DeliveryAddressLine5,
            command.Postcode
        )).GetAwaiter().GetResult();
        return base.Handle(command);
    }
}