



using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Logging.Attributes;
using Paramore.Brighter.Policies.Attributes;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.OrderManager.DataTransfer;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Policies;
using Saunter.Attributes;

namespace PlantBasedPizza.Deliver.Infrastructure.PublicEventHandlers;

[AsyncApi]
public class OrderReadyForDeliveryKafkaEventHandler(IServiceScopeFactory serviceScopeFactory, 
    IAmACommandProcessor processor)
    : RequestHandlerAsync<OrderReadyForDeliveryEventV1>
{
    [Channel("order-manager.ready-for-delivery")] // Creates a Channel
    [SubscribeOperation(typeof(OrderReadyForDeliveryEvent), Summary = "Handle an order ready for delivery event.", OperationId = "order-manager.ready-for-delivery")]
    [RequestLoggingAsync(step: 1, timing: HandlerTiming.Before)]
    // [UseResiliencePipeline(step: 2, policy: Retry.EXPONENTIAL_RETRYPOLICYASYNC)]
    public override async Task<OrderReadyForDeliveryEventV1> HandleAsync(OrderReadyForDeliveryEventV1 command,
        CancellationToken cancellationToken = new())
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<OrderReadyForDeliveryEventHandler>();

        await handler.Handle(new OrderReadyForDeliveryEvent(
            command.OrderIdentifier,
            command.DeliveryAddressLine1,
            command.DeliveryAddressLine2,
            command.DeliveryAddressLine3,
            command.DeliveryAddressLine4,
            command.DeliveryAddressLine5,
            command.Postcode
        ));
        return await base.HandleAsync(command);
    }

    public override async Task<OrderReadyForDeliveryEventV1> FallbackAsync(OrderReadyForDeliveryEventV1 command, 
        CancellationToken cancellationToken = new())
    {
        processor.Post(new DLQMessage(OrderReadyForDeliveryEventV1.EventTypeName)
        {
            Data = command.AsString(),
        });
        
        return await base.FallbackAsync(command, cancellationToken);
    }
}