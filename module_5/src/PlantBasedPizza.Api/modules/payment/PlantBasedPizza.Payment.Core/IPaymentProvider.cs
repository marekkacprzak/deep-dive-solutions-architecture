



namespace PlantBasedPizza.Payment.Core;

public interface IPaymentProvider
{
    Task<string> TakePaymentAsync(decimal amount);
}