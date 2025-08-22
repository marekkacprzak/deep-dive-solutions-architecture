



using PlantBasedPizza.Payment.DataTransfer;

namespace PlantBasedPizza.OrderManager.Core.Services;

public interface IPaymentService
{
    public Task<PaymentResultDTO> TakePaymentFor(Order order);
}