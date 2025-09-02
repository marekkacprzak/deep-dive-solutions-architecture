namespace PlantBasedPizza.OrderManager.Core.Handlers;

public class OrderQualityCheckedEventHandler(IOrderRepository orderRepository)
{
    public async Task Handle(OrderQualityCheckedEvent evt)
    {
        var order = await orderRepository.Retrieve(evt.OrderIdentifier);

        order.AddHistory("Order quality checked");

        if (order.OrderType == OrderType.Delivery)
        {
            order.ReadyForDelivery();
        }
        else
        {
            order.MarkAsAwaitingCollection();
        }

        await orderRepository.Update(order).ConfigureAwait(false);
    }
}