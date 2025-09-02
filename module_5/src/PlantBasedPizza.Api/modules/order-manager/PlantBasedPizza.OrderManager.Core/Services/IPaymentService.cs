namespace PlantBasedPizza.OrderManager.Core.Services;

public interface IPaymentService
{
    public Task<PaymentResult> TakePaymentFor(Order order);
}