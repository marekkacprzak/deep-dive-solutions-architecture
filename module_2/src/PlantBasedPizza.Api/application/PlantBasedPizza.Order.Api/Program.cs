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
var applicationName = "PlantBasedPizza-Order.Api";
builder.Services.AddOrderManagerInfrastructure(builder.Configuration, overrideConnectionString)
    .AddPaymentInfrastructure()
    .AddSharedInfrastructure(builder.Configuration, applicationName)
    .AddMessageProducers(builder.Configuration, applicationName, new List<string>())
    .AddHttpClient();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Request.Headers.Host.ToString(),
            partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 60,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});
builder.Services.AddResponseCompression(options => { options.EnableForHttps = true; });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowAll");

app.Map("/health", async () =>
{
    logger.Information("Health check requested");
    
    var serviceScopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
    
    using var scope = serviceScopeFactory.CreateScope();
    
    var ordersDbContext = scope.ServiceProvider.GetRequiredService<OrderManagerDbContext>();
    var ordersConnectionState = await ordersDbContext.Database.CanConnectAsync();
    
    return Results.Ok(new
    {
        ordersState = ordersConnectionState
    });
});

app.MapControllers();

await app.RunAsync();