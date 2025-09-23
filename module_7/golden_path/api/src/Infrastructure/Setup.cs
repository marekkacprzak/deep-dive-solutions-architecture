using Core;
using Core.Handlers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class Setup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration, string? overrideConnectionString = null)
    {
        // Register strongly typed configuration
        services.Configure<ApplicationOptions>(configuration.GetSection(ApplicationOptions.SectionName));
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("cache");
            options.InstanceName = "SharedCache";
        });

        // Register DbContext
        services.AddDbContext<DbContext>(options =>
            options.UseNpgsql(
                overrideConnectionString ?? configuration.GetConnectionString("Database"),
                b => b.MigrationsAssembly("PlantBasedPizza.OrderManager.Infrastructure")));
        services.AddServiceDiscovery();
        services.AddSingleton<ExampleEventPublisher, DistributedEventPublisher>();

        services.AddScoped<ExampleEventHandler>();

        return services;
    }
}