



using PlantBasedPizza.OrderManager.Core;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Payment.Core;
using PlantBasedPizza.Payment.DataTransfer;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class PaymentService(PlantBasedPizza.Payment.DataTransfer.PaymentService paymentService) : IPaymentService
{
    public Task<PaymentResultDTO> TakePaymentFor(Order order)
    {
        return paymentService.TakePayment(new TakePaymentCommand
        {
            OrderIdentifier = order.OrderIdentifier,
            Amount = order.TotalPrice
        });
    }
}