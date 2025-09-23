using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Deliver.Core;
using PlantBasedPizza.Deliver.Core.AssignDriver;
using PlantBasedPizza.Deliver.Core.Configuration;
using PlantBasedPizza.Deliver.Core.GetDelivery;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Deliver.Core.MarkOrderDelivered;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Deliver.Infrastructure
{
    public static class Setup
    {
        public static IServiceCollection AddDeliveryModuleInfrastructure(this IServiceCollection services,
            IConfiguration configuration, string? overrideConnectionString = null)
        {
            // Register strongly typed configuration
            services.Configure<DeliveryOptions>(configuration.GetSection(DeliveryOptions.SectionName));

            // Register DbContext
            services.AddDbContext<DeliveryDbContext>(options =>
                options.UseNpgsql(
                    overrideConnectionString ?? configuration.GetConnectionString("Database"),
                    b => b.MigrationsAssembly("PlantBasedPizza.Deliver.Infrastructure")));

            services.AddSingleton<IDeliveryEventPublisher, PublicEventPublisher>();
            services.AddScoped<IDeliveryRequestRepository, DeliveryRequestRepositoryPostgres>();
            services.AddScoped<OrderReadyForDeliveryEventHandler>();
            services.AddTransient<GetDeliveryQueryHandler>();
            
            // Domain services and factories
            services.AddSingleton<Handles<OrderDeliveredEvent>, DomainEventPublisher>();
            services.AddSingleton<Handles<DriverCollectedOrderEvent>, DomainEventPublisher>();
            services.AddTransient<Core.Services.IDeliveryRequestFactory, Core.Services.DeliveryRequestFactory>();
            services.AddTransient<Core.Services.IDeliveryDomainService, Core.Services.DeliveryDomainService>();

            // Register validators
            services.AddScoped<IValidator<AssignDriverRequest>, AssignDriverRequestValidator>();
            services.AddScoped<IValidator<MarkOrderDeliveredRequest>, MarkOrderDeliveredRequestValidator>();

            return services;
        }
    }
}