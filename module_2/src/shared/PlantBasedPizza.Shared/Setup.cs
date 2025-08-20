using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.Kafka;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;
using PlantBasedPizza.Shared.Events;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace PlantBasedPizza.Shared;

public static class Setup
{
    private const string OTEL_DEFAULT_GRPC_ENDPOINT = "http://localhost:4317";

    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services,
        IConfiguration configuration, string applicationName)

    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
            .Enrich.FromLogContext()
            .WriteTo.Console(new JsonFormatter());

        var otel = services.AddOpenTelemetry();
        otel.ConfigureResource(resource => resource
            .AddService(applicationName));

        otel.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation();
            tracing.AddRedisInstrumentation();
            tracing.AddEntityFrameworkCoreInstrumentation();
            tracing.AddSource(applicationName);
            tracing.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint =
                    new Uri(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? OTEL_DEFAULT_GRPC_ENDPOINT);
            });
        });

        services.AddHttpContextAccessor();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }

    public static IServiceCollection AddMessageConsumers<T>(this IServiceCollection services,
        IConfiguration configuration, string applicationName, EventSubscription<T>[] subscriptions) where T : IRequest
    {
        if (configuration.GetValue<bool>("UseDistributedServices"))
        {
            // Kafka consumer configuration
            var consumerFactory = new KafkaMessageConsumerFactory(new KafkaMessagingGatewayConfiguration
            {
                Name = applicationName,
                BootStrapServers = new[] { configuration["Messaging:Kafka"] },
                SecurityProtocol = SecurityProtocol.Plaintext,
                SaslMechanisms = SaslMechanism.Plain
            });

            services.AddServiceActivator(options =>
                {
                    options.ChannelFactory = new ChannelFactory(consumerFactory);
                    options.Subscriptions = subscriptions;
                })
                .AutoFromAssemblies()
                .AsyncHandlersFromAssemblies();
            services.AddHostedService<ServiceActivatorHostedService>();

            var dlqNames = new List<string>();
            
            // Configure dead letter queues
            foreach (var subscription in subscriptions)
            {
                var routingKey = subscription.RoutingKey;
                var deadLetterQueue = new RoutingKey($"{routingKey}.deadletter");
                
                dlqNames.Add(deadLetterQueue);
            }
            
            services.AddMessageProducers(configuration, applicationName, dlqNames);
        }

        return services;
    }

    public static IServiceCollection AddMessageProducers(this IServiceCollection services,
        IConfiguration configuration, string applicationName, List<string>? messageTopics, params Assembly[] mapperAssemblies)
    {
        if (configuration.GetValue<bool>("UseDistributedServices"))
            services.AddBrighter()
                .UseExternalBus(new KafkaProducerRegistryFactory(
                        new KafkaMessagingGatewayConfiguration
                        {
                            Name = applicationName,
                            BootStrapServers = new[] { configuration["Messaging:Kafka"] },
                            SecurityProtocol = SecurityProtocol.Plaintext,
                            SaslMechanisms = SaslMechanism.Plain
                        },
                        (messageTopics ?? []).Select(topic => new KafkaPublication
                        {
                            Topic = new RoutingKey(topic),
                            NumPartitions = 3,
                            ReplicationFactor = 3,
                            MessageTimeoutMs = 1000,
                            RequestTimeoutMs = 1000,
                            MakeChannels = OnMissingChannel.Create
                        })
                    )
                    .Create())
                .AutoFromAssemblies(mapperAssemblies)
                .AsyncHandlersFromAssemblies();

        return services;
    }
}