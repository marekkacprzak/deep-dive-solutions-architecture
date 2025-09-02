namespace PlantBasedPizza.OrderManager.Core.Handlers;

public class OrderPrepCompleteEventHandler(
    IOrderRepository orderRepository)
{
    public async Task Handle(OrderPrepCompleteEvent evt)
    {
        var order = await orderRepository.Retrieve(evt.OrderIdentifier);

        order.AddHistory("Order prep completed");

        await orderRepository.Update(order);
    }
}