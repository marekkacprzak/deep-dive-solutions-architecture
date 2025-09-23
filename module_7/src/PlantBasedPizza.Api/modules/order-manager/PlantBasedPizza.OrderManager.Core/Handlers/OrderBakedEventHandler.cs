namespace PlantBasedPizza.OrderManager.Core.Handlers
{
    public class OrderBakedEventHandler(IOrderRepository orderRepository)
    {
        public async Task Handle(OrderBakedEvent evt)
        {
            var order = await orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory("Order baked");

            await orderRepository.Update(order);
        }
    }
}