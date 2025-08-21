



using PlantBasedPizza.OrderManager.Core;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Payment.Core;
using PlantBasedPizza.Payment.DataTransfer;
using PaymentResult = PlantBasedPizza.OrderManager.Core.Services.PaymentResult;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class PaymentService(PlantBasedPizza.Payment.DataTransfer.PaymentService paymentService) : IPaymentService
{
    public async Task<PaymentResult> TakePaymentFor(Order order)
    {
        var paymentResult = await paymentService.TakePayment(new TakePaymentCommand
        {
            OrderIdentifier = order.OrderIdentifier,
            Amount = order.TotalPrice
        });

        return new PaymentResult(paymentResult.PaymentId);
    }
}