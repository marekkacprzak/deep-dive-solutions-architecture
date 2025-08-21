



namespace PlantBasedPizza.OrderManager.Core.MarkAwaitingCollection;

public class MarkAwaitingCollectionCommandHandler(IOrderRepository orderRepository)
{
    public async Task<OrderDto?> Handle(MarkAwaitingCollectionCommand request)
    {
        try
        {
            var order = await orderRepository.Retrieve(request.OrderIdentifier);
            
            ArgumentNullException.ThrowIfNull(order);

            order.MarkAsAwaitingCollection();
            order.AddHistory("Order awaiting collection");

            // Note: No event needed for this operation based on current domain logic

            await orderRepository.Update(order);

            return new OrderDto(order);
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}