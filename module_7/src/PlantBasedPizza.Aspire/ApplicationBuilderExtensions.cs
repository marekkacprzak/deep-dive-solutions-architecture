using Aspire.Hosting;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System.Xml.Linq;

namespace PlantBasedPizza.Aspire;

public static class ApplicationBuilderExtensions
{
    public static IDistributedApplicationBuilder WithDistributedServices(
        this IDistributedApplicationBuilder builder)
    {
        var cache = builder.AddValkey("cache");
        
        var kafka = builder.AddKafka("kafka")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithKafkaUI()
            .WithLifetime(ContainerLifetime.Persistent);
        
        builder.Eventing.Subscribe<ResourceReadyEvent>(kafka.Resource, async (@event, ct) =>
        {
            var cs = await kafka.Resource.ConnectionStringExpression.GetValueAsync(ct);

            var config = new AdminClientConfig
            {
                BootstrapServers = cs
            };

            using var adminClient = new AdminClientBuilder(config).Build();
            try
            {
                await adminClient.CreateTopicsAsync(new TopicSpecification[]
                {
                    new TopicSpecification { Name = "orders.order-completed", NumPartitions = 1, ReplicationFactor = 1 },
                    new TopicSpecification { Name = "orders.order-created", NumPartitions = 1, ReplicationFactor = 1 },
                    new TopicSpecification { Name = "orders.order-ready-for-delivery", NumPartitions = 1, ReplicationFactor = 1 },
                    new TopicSpecification { Name = "orders.order-submitted", NumPartitions = 1, ReplicationFactor = 1 },
                    new TopicSpecification { Name = "kitchen.baked", NumPartitions = 1, ReplicationFactor = 1 },
                    new TopicSpecification { Name = "kitchen.prep-started", NumPartitions = 1, ReplicationFactor = 1 },
                    new TopicSpecification { Name = "kitchen.prep-complete", NumPartitions = 1, ReplicationFactor = 1 },
                    new TopicSpecification { Name = "kitchen.quality-checked", NumPartitions = 1, ReplicationFactor = 1 },
                    new TopicSpecification { Name = "delivery.driver-collected", NumPartitions = 1, ReplicationFactor = 1 },
                    new TopicSpecification { Name = "delivery.order-delivered", NumPartitions = 1, ReplicationFactor = 1 },
                    new TopicSpecification { Name = "recipe.recipe-created", NumPartitions = 1, ReplicationFactor = 1 },
                });
            }
            catch (CreateTopicsException e)
            {
                Console.WriteLine($"An error occurred creating topic: {e.Message}");
                throw;
            }
        });

        var recipesDb = builder
            .AddPostgres("recipesDb")
            .WithLifetime(ContainerLifetime.Persistent)
            .AddDatabase("plantbasedpizza-recipes");
        var recipeApplication = builder.AddProject<Projects.PlantBasedPizza_Recipes_Api>("recipe-api")
            .WithReference(recipesDb)
            .WithReference(kafka)
            .WithReference(cache)
            .WithEnvironment("Messaging__Kafka", kafka)
            .WithEnvironment("ConnectionStrings__Database", recipesDb)
            .WithHttpEndpoint(8084)
            .WaitFor(recipesDb)
            .WaitFor(kafka);

        var orderDb = builder
            .AddPostgres("orderDb")
            .WithLifetime(ContainerLifetime.Persistent)
            .AddDatabase("plantbasedpizza-orders");
        var orderManagerApplication = builder.AddProject<Projects.PlantBasedPizza_Order_Api>("order-manager-api")
            .WithReference(orderDb)
            .WithReference(recipeApplication)
            .WithReference(kafka)
            .WithReference(cache)
            .WithEnvironment("Messaging__Kafka", kafka)
            .WithEnvironment("Services__RecipeApi", "http://recipe-api")
            .WithEnvironment("ConnectionStrings__Database", orderDb)
            .WithHttpEndpoint(8081)
            .WaitFor(orderDb)
            .WaitFor(kafka);
        var orderManagerWorker = builder.AddProject<Projects.PlantBasedPizza_Worker>("order-manager-worker")
            .WithReference(orderDb)
            .WithReference(recipeApplication)
            .WithReference(kafka)
            .WithReference(cache)
            .WithEnvironment("Messaging__Kafka", kafka)
            .WithEnvironment("Services__RecipeApi", "http://recipe-api")
            .WithEnvironment("ConnectionStrings__Database", orderDb)
            .WithHttpEndpoint(8085)
            .WaitFor(orderDb)
            .WaitFor(kafka);

        var kitchenDb = builder
            .AddPostgres("kitchenDb")
            .WithLifetime(ContainerLifetime.Persistent)
            .AddDatabase("plantbasedpizza-kitchen");
        var kitchenApplication = builder.AddProject<Projects.PlantBasedPizza_Kitchen_Api>("kitchen-api")
            .WithReference(kitchenDb)
            .WithReference(orderManagerApplication)
            .WithReference(recipeApplication)
            .WithReference(kafka)
            .WithReference(cache)
            .WithEnvironment("Messaging__Kafka", kafka)
            .WithEnvironment("Services__RecipeApi", "http://recipe-api")
            .WithEnvironment("Services__OrderApi", "http://order-manager-api")
            .WithEnvironment("ConnectionStrings__Database", kitchenDb)
            .WithHttpEndpoint(8082)
            .WaitFor(kitchenDb)
            .WaitFor(kafka);

        var deliveryDb = builder
            .AddPostgres("deliveryDb")
            .WithLifetime(ContainerLifetime.Persistent)
            .AddDatabase("plantbasedpizza-delivery");
        var deliveryApplication = builder.AddProject<Projects.PlantBasedPizza_Delivery_Api>("delivery-api")
            .WithReference(deliveryDb)
            .WithReference(kafka)
            .WithEnvironment("Messaging__Kafka", kafka)
            .WithEnvironment("ConnectionStrings__Database", deliveryDb)
            .WithHttpEndpoint(8083)
            .WaitFor(deliveryDb)
            .WaitFor(kafka);

        var yarpProxy = builder.AddProject<Projects.PlantBasedPizza_Gateway>("gateway")
            .WithReference(orderManagerApplication)
            .WithReference(kitchenApplication)
            .WithReference(recipeApplication)
            .WithReference(deliveryApplication);

        builder.Eventing.Subscribe<ResourceReadyEvent>(yarpProxy.Resource, async (@event, ct) =>
        {
            await yarpProxy.Resource.UpdateTestEndpoint();
        });

        return builder;
    }

}