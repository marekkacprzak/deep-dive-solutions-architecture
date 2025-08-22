



using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Payment.Core;
using PlantBasedPizza.Payment.DataTransfer;

namespace PlantBasedPizza.Payment.Infrastructure;

public static class Setup
{
    public static IServiceCollection AddPaymentInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPaymentProvider, NoOpPaymentProvider>();
        services.AddSingleton<TakePaymentCommandHandler>();
        services.AddSingleton<PaymentService>();

        return services;
    }
    
}