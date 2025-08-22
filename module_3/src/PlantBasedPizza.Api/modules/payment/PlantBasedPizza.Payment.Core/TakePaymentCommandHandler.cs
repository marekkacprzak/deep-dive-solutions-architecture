



using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;

namespace PlantBasedPizza.Payment.Core;

public class TakePaymentCommandHandler(IPaymentProvider provider, IDistributedCache cache, ActivitySource activitySource)
{
    public async Task<PaymentResult> Handle(TakePaymentCommand command)
    {
        using var paymentActivity = activitySource.StartActivity("takePayment");
        paymentActivity?.SetTag("order.identifier", command.OrderIdentifier);
        paymentActivity?.SetTag("payment.amount", command.Amount);
;
        if (command.Amount <= 0)
        {
            return new  PaymentResult()
            {
                Message = "Payment amount must be greater than zero.",
                PaymentId = string.Empty
            };
        }

        var cachedPayment = await cache.GetStringAsync($"payment:{command.OrderIdentifier}");

        if (cachedPayment != null)
        {
            paymentActivity?.SetTag("payment.idempotent", "true");
        }
        
        paymentActivity?.SetTag("payment.idempotent", "false");

        var paymentResult = await provider.TakePaymentAsync(command.Amount);
        
        paymentActivity?.SetTag("payment.paymentId", paymentResult);

        return new PaymentResult()
        {
            Message = "OK",
            PaymentId = paymentResult
        };
    }
}