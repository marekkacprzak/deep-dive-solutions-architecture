using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.Handlers;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Kitchen.Infrastructure
{
    public static class Setup
    {
        public static IServiceCollection AddKitchenInfrastructure(this IServiceCollection services,
            IConfiguration configuration, string? overrideConnectionString = null)
        {
            // Register DbContext
            services.AddDbContext<KitchenDbContext>(options =>
                options.UseNpgsql(
                    overrideConnectionString ?? configuration.GetConnectionString("KitchenPostgresConnection"),
                    b => b.MigrationsAssembly("PlantBasedPizza.Kitchen.Infrastructure")));

            if (configuration.GetValue<bool>("UseDistributedServices")){
                services.AddServiceDiscovery();
                var recipeServiceEndpoint = configuration["Services:RecipeApi"];
                ArgumentNullException.ThrowIfNullOrEmpty(recipeServiceEndpoint);
                var orderServiceEndpoint = configuration["Services:OrderApi"];
                ArgumentNullException.ThrowIfNullOrEmpty(orderServiceEndpoint);

                services.AddHttpClient<IRecipeService, HttpRecipeService>(client =>
                {
                    client.BaseAddress = new Uri(recipeServiceEndpoint, UriKind.Absolute);
                })
                .AddServiceDiscovery();
                services.AddHttpClient<IOrderManagerService, HttpOrderService>(client =>
                {
                    client.BaseAddress = new Uri(orderServiceEndpoint, UriKind.Absolute);
                })
                .AddServiceDiscovery();
            }
            else
            {
                services.AddTransient<IRecipeService, RecipeService>();
                services.AddTransient<IOrderManagerService, OrderManagerService>();
            }

            services.AddTransient<Handles<OrderSubmittedEvent>, OrderSubmittedEventHandler>();
            services.AddScoped<OrderSubmittedEventHandler>();
            services.AddScoped<IKitchenRequestRepository, KitchenRequestRepositoryPostgres>();

            return services;
        }
    }
}