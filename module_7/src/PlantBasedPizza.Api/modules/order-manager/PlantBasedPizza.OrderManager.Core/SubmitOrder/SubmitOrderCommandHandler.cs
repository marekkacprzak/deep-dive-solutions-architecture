using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Core.SubmitOrder;

public class SubmitOrderCommandHandler(IOrderRepository orderRepository, IPaymentService paymentService)
{
    public async Task<OrderDto?> Handle(SubmitOrderCommand request)
    {
        try
        {
            var order = await orderRepository.Retrieve(request.OrderIdentifier);
            
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