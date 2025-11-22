using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PlantBasedPizza.Recipes.DataTransfer;
using PlantBasedPizza.Recipes.Infrastructure;
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

var applicationName = "recipe-service";

// Connect to a PostgreSQL database.
builder.Services.AddRecipeInfrastructure(builder.Configuration, logger, overrideConnectionString)
    .AddSharedInfrastructure(builder.Configuration, applicationName)
    .AddMessageProducers(builder.Configuration, applicationName, new List<PublicEvent>(1)
    {
        new RecipeCreatedEventV1("", "", 0),
    }, typeof(RecipeCreatedEventV1).Assembly)
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
    var recipesDbContext = scope.ServiceProvider.GetRequiredService<RecipesDbContext>();
    await recipesDbContext.Database.MigrateAsync();
}

app.Map("/health", async () =>
{
    logger.Information("Health check requested");
    
    var serviceScopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
    
    using var scope = serviceScopeFactory.CreateScope();

    var recipesDbContext = scope.ServiceProvider.GetRequiredService<RecipesDbContext>();
    var recipesConnectionState = await recipesDbContext.Database.CanConnectAsync();
    
    return Results.Ok(new
    {
        recipesState = recipesConnectionState,
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