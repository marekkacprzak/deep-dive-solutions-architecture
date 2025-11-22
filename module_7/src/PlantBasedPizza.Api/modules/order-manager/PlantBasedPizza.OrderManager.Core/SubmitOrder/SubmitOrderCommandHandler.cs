using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Core.SubmitOrder;

public class SubmitOrderCommandHandler(IOrderRepository orderRepository, IPaymentService paymentService)
{
    public async Task<OrderDto?> Handle(SubmitOrderCommand request)
    {
        try
        {
            if (request.OrderIdentifier is null)
            {
                throw new ArgumentException("OrderIdentifier cannot be null");
            }

            var order = await orderRepository.Retrieve(request.OrderIdentifier);
            
            if (order is null)
            {
                return null;
            }
            
            var takePayment = await paymentService.TakePaymentFor(order);

            if (string.IsNullOrEmpty(takePayment.PaymentId))
            {
                return null;
            }

            ArgumentNullException.ThrowIfNull(order);

            if (!order.Items.Any()) 
                throw new ArgumentException("Cannot submit an order with no items");

            order.MarkAsSubmitted();
            order.AddHistory("Submitted order.");

            await orderRepository.Update(order);

            return new OrderDto(order);
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}