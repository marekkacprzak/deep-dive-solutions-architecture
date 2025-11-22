using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.Inbox.Postgres;
using Paramore.Brighter.MessagingGateway.Kafka;
using Paramore.Brighter.Observability;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Policies;
using Polly;
using Polly.Registry;
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

    public static IServiceCollection AddMessageConsumers(this IServiceCollection services,
        IConfiguration configuration, string applicationName, Subscription[] subscriptions,
        params Assembly[] mapperAssemblies)
    {
        if (subscriptions.Length == 0) return services;

        // Kafka consumer configuration
        var consumerFactory = new KafkaMessageConsumerFactory(new KafkaMessagingGatewayConfiguration
        {
            Name = applicationName,
            BootStrapServers = new[] { configuration["Messaging:Kafka"] },
            SecurityProtocol = SecurityProtocol.Plaintext,
            SaslMechanisms = SaslMechanism.Plain
        });

        var requestTypes = GetRequestTypes(mapperAssemblies);

        services.AddConsumers(options =>
            {
                options.InboxConfiguration = new InboxConfiguration(
                    new InMemoryInbox(TimeProvider.System),
                    InboxScope.All, true, OnceOnlyAction.Throw);
                options.DefaultChannelFactory = new ChannelFactory(consumerFactory);
                options.ResiliencePipelineRegistry = new ResiliencePipelineRegistry<string>()
                    .AddBrighterDefault()
                    .AddDefaultRetries(requestTypes);
                options.InstrumentationOptions = InstrumentationOptions.All;
                options.Subscriptions = subscriptions;
            })
            .ConfigureJsonSerialisation(options => { options.PropertyNameCaseInsensitive = true; })
            .AutoFromAssemblies(mapperAssemblies);

        services.AddHostedService<ServiceActivatorHostedService>();

        var dlqNames = new List<PublicEvent>();

        // Configure dead letter queues
        foreach (var subscription in subscriptions)
        {
            var routingKey = subscription.RoutingKey;
            var deadLetterQueue = new RoutingKey($"{routingKey}.deadletter");

            dlqNames.Add(new DLQMessage(deadLetterQueue));
        }

        services.AddMessageProducers(configuration, applicationName, dlqNames);

        return services;
    }

    public static IServiceCollection AddMessageProducers(this IServiceCollection services,
        IConfiguration configuration, string applicationName, List<PublicEvent>? messageTopics,
        params Assembly[] mapperAssemblies)
    {
        var requestTypes = GetRequestTypes(mapperAssemblies);

        var brighter = services.AddBrighter(options =>
        {
            options.InstrumentationOptions = InstrumentationOptions.All;
            options.ResiliencePipelineRegistry = new ResiliencePipelineRegistry<string>()
                .AddBrighterDefault()
                .AddDefaultRetries(requestTypes);
        });

        if (messageTopics is null || !messageTopics.Any()) return services;

        brighter.AddProducers(options =>
            {
                options.MaxOutStandingMessages = 5;
                options.MaxOutStandingCheckInterval = TimeSpan.FromMilliseconds(500);
                options.InstrumentationOptions = InstrumentationOptions.All;
                options.Outbox = new InMemoryOutbox(TimeProvider.System, InstrumentationOptions.All);
                options.ProducerRegistry =
                    GetKafkaProducerRegistry(configuration, applicationName, messageTopics ?? []);
            })
            .ConfigureJsonSerialisation(options => { options.PropertyNameCaseInsensitive = true; })
            .AutoFromAssemblies(mapperAssemblies);

        return services;
    }

    private static IEnumerable<Type> GetRequestTypes(Assembly[] assemblies)
    {
        return assemblies.SelectMany(a => a.GetTypes())
            .Where(t => typeof(IRequest).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
    }

    private static IAmAProducerRegistry GetKafkaProducerRegistry(IConfiguration configuration, string applicationName,
        List<PublicEvent> messageTopics)
    {
        var producerRegistry = new KafkaProducerRegistryFactory(
            new KafkaMessagingGatewayConfiguration
            {
                Name = applicationName,
                BootStrapServers = new[] { configuration["Messaging:Kafka"] },
                SecurityProtocol = SecurityProtocol.Plaintext,
                SaslMechanisms = SaslMechanism.Plain
            },
            messageTopics.Select(topic =>
                new KafkaPublication
                {
                    DataSchema = null,
                    Topic = new RoutingKey(topic.EventName),
                    RequestType = topic.GetType(),
                    EnableIdempotence = true,
                    NumPartitions = 3,
                    Partitioner = Partitioner.Random,
                    ReplicationFactor = 3,
                    MessageTimeoutMs = 1000,
                    RequestTimeoutMs = 1000,
                    MakeChannels = OnMissingChannel.Create
                })).Create();

        return producerRegistry;
    }
}