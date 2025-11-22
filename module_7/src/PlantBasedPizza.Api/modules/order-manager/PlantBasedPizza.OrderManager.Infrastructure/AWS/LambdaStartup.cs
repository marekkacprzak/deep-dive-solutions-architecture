using Amazon.Lambda.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.OrderManager.DataTransfer;
using PlantBasedPizza.Payment.Infrastructure;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Events;
using Serilog;
using Serilog.Formatting.Json;

namespace PlantBasedPizza.OrderManager.Infrastructure.AWS;

[LambdaStartup]
public class LambdaStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var logger = Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console(new JsonFormatter())
            .CreateLogger();

        var applicationName = "OrdersWorker-Lambda";
        var overrideConnectionString = Environment.GetEnvironmentVariable("OVERRIDE_CONNECTION_STRING");

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        services.AddMessageProducers(configuration, applicationName, new List<PublicEvent>(3)
            {
                new OrderCreatedEventV1(""),
                new OrderSubmittedEventV1(""),
                new OrderCompletedEventV1("")
            }, typeof(OrderCreatedEventV1).Assembly)
            .AddOrderManagerInfrastructure(configuration, overrideConnectionString)
            .AddPaymentInfrastructure()
            .AddSharedInfrastructure(configuration, applicationName)
            .AddHttpClient();
    }
}