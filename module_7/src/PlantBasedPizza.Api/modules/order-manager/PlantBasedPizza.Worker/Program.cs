using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Paramore.Brighter;
using PlantBasedPizza.Delivery.DataTransfer;
using PlantBasedPizza.Kitchen.DataTransfer;
using PlantBasedPizza.OrderManager.DataTransfer;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Payment.Infrastructure;
using PlantBasedPizza.Recipes.DataTransfer;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Worker;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);
var logger = Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();
builder.Host.UseSerilog((_, config) =>
{
    config.MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console(new JsonFormatter());
});

builder
    .Configuration
    .AddEnvironmentVariables();

var overrideConnectionString = Environment.GetEnvironmentVariable("OVERRIDE_CONNECTION_STRING");

// Connect to a PostgreSQL database.
var applicationName = "PlantBasedPizza-Order.Worker";

builder.Services
    .AddMessageProducers(builder.Configuration, applicationName, new List<PublicEvent>(4)
    {
        new OrderCreatedEventV1(""),
        new OrderSubmittedEventV1(""),
        new OrderCompletedEventV1(""),
        new OrderReadyForDeliveryEventV1("", "", "", "", "", "", "")
    }, Assembly.Load("PlantBasedPizza.OrderManager.DataTransfer"))
    .AddMessageConsumers(builder.Configuration, applicationName, new Subscription[7]
    {
        new EventSubscription<OrderPreparingEventV1>(applicationName, "order.orderPreparing",
            OrderPreparingEventV1.EventTypeName),
        new EventSubscription<OrderPrepCompleteEventV1>(applicationName, "order.orderPreparing",
            OrderPrepCompleteEventV1.EventTypeName),
        new EventSubscription<OrderBakedEventV1>(applicationName, "order.orderPreparing",
            OrderBakedEventV1.EventTypeName),
        new EventSubscription<OrderQualityCheckedEventV1>(applicationName, "order.orderPreparing",
            OrderQualityCheckedEventV1.EventTypeName),
        new EventSubscription<DriverCollectedOrderEventV1>(applicationName, "order.driverCollectedOrder",
            DriverCollectedOrderEventV1.EventTypeName),
        new EventSubscription<OrderDeliveredEventV1>(applicationName, "order.orderDelivered",
            OrderDeliveredEventV1.EventTypeName),
        new EventSubscription<RecipeCreatedEventV1>(applicationName, "order.orderDelivered",
            RecipeCreatedEventV1.EventTypeName),
    }, 
    Assembly.Load("PlantBasedPizza.Kitchen.DataTransfer"),
    Assembly.Load("PlantBasedPizza.Delivery.DataTransfer"),
    Assembly.Load("PlantBasedPizza.Recipes.DataTransfer"))
    .AddOrderManagerInfrastructure(builder.Configuration, overrideConnectionString)
    .AddPaymentInfrastructure()
    .AddSharedInfrastructure(builder.Configuration, applicationName)
    .AddHttpClient();

builder.Services.AddHostedService<OrderOutboxWorker>();

var app = builder.Build();

var serviceScopeFactory = app.Services.GetService<IServiceScopeFactory>();
using (var scope = serviceScopeFactory.CreateScope())
{
    var ordersDbContext = scope.ServiceProvider.GetRequiredService<OrderManagerDbContext>();
    await ordersDbContext.Database.MigrateAsync();
}

await app.RunAsync();