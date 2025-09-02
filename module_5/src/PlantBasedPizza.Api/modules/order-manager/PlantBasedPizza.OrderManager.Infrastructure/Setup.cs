using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.OrderManager.Core;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;
using PlantBasedPizza.OrderManager.Core.CollectOrder;
using PlantBasedPizza.OrderManager.Core.CompleteOrder;
using PlantBasedPizza.OrderManager.Core.Configuration;
using PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;
using PlantBasedPizza.OrderManager.Core.CreatePickupOrder;
using PlantBasedPizza.OrderManager.Core.Handlers;
using PlantBasedPizza.OrderManager.Core.MarkAwaitingCollection;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.OrderManager.Core.SubmitOrder;
using PlantBasedPizza.OrderManager.DataTransfer;
using PlantBasedPizza.Shared.Policies;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public static class Setup
{
    public static IServiceCollection AddOrderManagerInfrastructure(this IServiceCollection services,
        IConfiguration configuration, string? overrideConnectionString = null)
    {
        // Register strongly typed configuration
        services.Configure<OrderOptions>(configuration.GetSection(OrderOptions.SectionName));
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("cache");
            options.InstanceName = "SharedCache";
        });

        // Register DbContext
        services.AddDbContext<OrderManagerDbContext>(options =>
            options.UseNpgsql(
                overrideConnectionString ?? configuration.GetConnectionString("Database"),
                b => b.MigrationsAssembly("PlantBasedPizza.OrderManager.Infrastructure")));

        services.AddTransient<OrderManagerDataTransferService>();
        services.AddScoped<IOrderRepository, OrderRepositoryPostgres>();
        services.AddTransient<CollectOrderCommandHandler>();
        services.AddTransient<AddItemToOrderHandler>();
        services.AddTransient<SubmitOrderCommandHandler>();
        services.AddTransient<CompleteOrderCommandHandler>();
        services.AddTransient<MarkAwaitingCollectionCommandHandler>();
        services.AddTransient<CreateDeliveryOrderCommandHandler>();
        services.AddTransient<CreatePickupOrderCommandHandler>();

        var recipeServiceEndpoint = configuration["Services:RecipeApi"];
        ArgumentNullException.ThrowIfNullOrEmpty(recipeServiceEndpoint);

        services.AddServiceDiscovery();

        services.AddHttpClient<IRecipeService, HttpRecipeService>(client =>
            {
                client.BaseAddress = new Uri(recipeServiceEndpoint, UriKind.Absolute);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(Retry.DefaultHttpRetryPolicy())
            .AddServiceDiscovery();
        services.AddSingleton<OrderEventPublisher, DistributedEventPublisher>();

        services.AddTransient<IPaymentService, PaymentService>();

        // Domain services and factories
        services.AddTransient<IOrderFactory, OrderFactory>();

        // Register validators
        services.AddScoped<IValidator<CreatePickupOrderCommand>, CreatePickupOrderCommandValidator>();
        services.AddScoped<IValidator<CreateDeliveryOrderCommand>, CreateDeliveryOrderCommandValidator>();
        services.AddScoped<IValidator<AddItemToOrderCommand>, AddItemToOrderCommandValidator>();

        services.AddScoped<OrderPreparingEventHandler>();
        services.AddScoped<OrderPrepCompleteEventHandler>();
        services.AddScoped<OrderBakedEventHandler>();
        services.AddScoped<OrderQualityCheckedEventHandler>();
        services.AddScoped<DriverDeliveredOrderEventHandler>();
        services.AddScoped<DriverCollectedOrderEventHandler>();

        return services;
    }
}