



using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.Core.CompleteOrder;

public class CompleteOrderCommandHandler(IOrderRepository orderRepository, IDomainEventDispatcher eventDispatcher)
{
    public async Task<OrderDto?> Handle(CompleteOrderCommand request)
    {
        try
        {
            var order = await orderRepository.Retrieve(request.OrderIdentifier);
            
            ArgumentNullException.ThrowIfNull(order);

            order.MarkAsCompleted();
            order.AddHistory("Order completed.");

            var evt = new OrderCompletedEvent(order.CustomerIdentifier, order.OrderIdentifier, order.TotalPrice)
            {
                CorrelationId = request.CorrelationId
            };

            await eventDispatcher.PublishAsync(evt);
            order.AddIntegrationEvent(evt);

            await orderRepository.Update(order);

            return new OrderDto(order);
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}