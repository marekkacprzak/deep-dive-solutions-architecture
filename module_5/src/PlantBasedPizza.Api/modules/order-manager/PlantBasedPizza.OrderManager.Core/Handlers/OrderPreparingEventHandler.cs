using Microsoft.Extensions.Logging;

namespace PlantBasedPizza.OrderManager.Core.Handlers;

public class OrderPreparingEventHandler(IOrderRepository orderRepository, ILogger<OrderPreparingEventHandler> logger)
{
    public async Task Handle(OrderPreparingEvent evt)
    {
        logger.LogInformation($"[ORDER-MANAGER] Handling order preparing event");

        var order = await orderRepository.Retrieve(evt.OrderIdentifier);

        order.AddHistory("Order prep started");

        await orderRepository.Update(order);
    }
}