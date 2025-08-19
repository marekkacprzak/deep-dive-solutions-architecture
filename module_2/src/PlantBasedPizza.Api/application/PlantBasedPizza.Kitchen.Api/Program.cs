using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Events;
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
var applicationName = "kitchen-service";

builder.Services
    .AddKitchenInfrastructure(builder.Configuration, overrideConnectionString)
    .AddSharedInfrastructure(builder.Configuration, applicationName)
    .AddMessageConsumers(builder.Configuration, applicationName, new[]
    {
        new EventSubscription<OrderSubmittedEvent>(applicationName, "kitchen.orderSubmitted", "orders.order-submitted")
    })
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

var serviceScopeFactory = app.Services.GetService<IServiceScopeFactory>();
using (var scope = serviceScopeFactory.CreateScope())
{
    var kitchenDbContext = scope.ServiceProvider.GetRequiredService<KitchenDbContext>();
    await kitchenDbContext.Database.MigrateAsync();
}

app.Map("/health", async () =>
{
    logger.Information("Health check requested");

    var serviceScopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

    using var scope = serviceScopeFactory.CreateScope();
    var kitchenDbContext = scope.ServiceProvider.GetRequiredService<KitchenDbContext>();
    var kitchenConnectionState = await kitchenDbContext.Database.CanConnectAsync();

    return Results.Ok(new
    {
        kitchenState = kitchenConnectionState
    });
});

app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    var correlationId = string.Empty;

    if (context.Request.Headers.ContainsKey(CorrelationContext.DefaultRequestHeaderName))
    {
        correlationId = context.Request.Headers[CorrelationContext.DefaultRequestHeaderName].ToString();
    }
    else
    {
        correlationId = Guid.NewGuid().ToString();

        context.Request.Headers.Append(CorrelationContext.DefaultRequestHeaderName, correlationId);
    }

    CorrelationContext.SetCorrelationId(correlationId);

    logger.LogInformation($"Request received to {context.Request.Path.Value}");

    context.Response.Headers.Append(CorrelationContext.DefaultRequestHeaderName, correlationId);

    // Do work that doesn't write to the Response.
    await next.Invoke();
});

app.MapControllers();

await app.RunAsync();