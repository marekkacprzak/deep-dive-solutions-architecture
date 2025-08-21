



using System.Diagnostics;

namespace PlantBasedPizza.Payment.Core;

public class TakePaymentCommandHandler(IPaymentProvider provider, ActivitySource activitySource)
{
    public async Task<PaymentResult> Handle(TakePaymentCommand command)
    {
        using var activity = activitySource.StartActivity("handle payment.takePayment");

        if (command.Amount <= 0)
        {
            return new  PaymentResult()
            {
                Message = "Payment amount must be greater than zero.",
                PaymentId = string.Empty
            };
        }

        var paymentResult = await provider.TakePaymentAsync(command.Amount);

        return new PaymentResult()
        {
            Message = "OK",
            PaymentId = paymentResult
        };
    }
}