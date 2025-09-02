using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Recipes.DataTransfer;

namespace PlantBasedPizza.Recipes.Infrastructure;

public static class Setup
{
    public static IServiceCollection AddRecipeInfrastructure(this IServiceCollection services,
        IConfiguration configuration, Serilog.ILogger logger, string? overrideConnectionString = null)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("cache");
            options.InstanceName = "PlantBasedPizzaRecipesCache";
        });
        
        // Register DbContext
        services.AddDbContext<RecipesDbContext>(options =>
            options.UseNpgsql(
                    overrideConnectionString ?? configuration.GetConnectionString("Database"),
                    b =>
                        b.MigrationsAssembly("PlantBasedPizza.Recipes.Infrastructure")
                            .EnableRetryOnFailure(
                                2,
                                TimeSpan.FromSeconds(2),
                                null))
                .LogTo(logger.Information));

        services.AddTransient<RecipeDataTransferService>();
        services.AddScoped<IRecipeRepository, RecipeRepositoryPostgres>();
        
        // Domain services and factories
        services.AddTransient<Core.Services.IRecipeFactory, Core.Services.RecipeFactory>();
        services.AddTransient<Core.Services.IRecipeDomainService, Core.Services.RecipeDomainService>();
        services.AddTransient<RecipeEventPublisher, DistributedEventPublisher>();

        return services;
    }
}