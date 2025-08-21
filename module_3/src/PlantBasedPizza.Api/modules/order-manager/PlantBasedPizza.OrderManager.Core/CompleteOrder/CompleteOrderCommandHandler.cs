



namespace PlantBasedPizza.OrderManager.Core.CompleteOrder;

public class CompleteOrderCommandHandler(IOrderRepository orderRepository)
{
    public async Task<OrderDto?> Handle(CompleteOrderCommand request)
    {
        try
        {
            var order = await orderRepository.Retrieve(request.OrderIdentifier);
            
            ArgumentNullException.ThrowIfNull(order);

            order.MarkAsCompleted();
            order.AddHistory("Order completed.");

            await orderRepository.Update(order);

            return new OrderDto(order);
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}