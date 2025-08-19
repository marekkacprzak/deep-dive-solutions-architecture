using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Payment.Infrastructure;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Logging;
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
    .AddMessageProducers(builder.Configuration, applicationName, new List<string>(3)
    {
        "orders.order-created",
        "orders.order-submitted",
        "orders.order-completed"
    })
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