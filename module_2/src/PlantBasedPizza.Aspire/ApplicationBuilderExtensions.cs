using Aspire.Hosting.Yarp;
using Microsoft.AspNetCore.Builder;

namespace PlantBasedPizza.Aspire;

public static class ApplicationBuilderExtensions
{
    public static IDistributedApplicationBuilder WithSingleApplicationInstance(
        this IDistributedApplicationBuilder builder)
    {
        var db = builder
            .AddPostgres("database")
            .WithLifetime(ContainerLifetime.Persistent)
            .AddDatabase("plantbasedpizza");

        var api = builder.AddProject<Projects.PlantBasedPizza_Api>("api")
            .WithReference(db)
            .WithEnvironment("ConnectionStrings__RecipesPostgresConnection", db)
            .WithEnvironment("ConnectionStrings__OrderManagerPostgresConnection", db)
            .WithEnvironment("ConnectionStrings__KitchenPostgresConnection", db)
            .WithEnvironment("ConnectionStrings__DeliveryPostgresConnection", db)
            .WithHttpEndpoint(8081)
            .WaitFor(db);

        var yarpProxy = builder.AddYarp("gateway")
            .WithConfigFile("yarp.single.json")
            .WithReference(api);

        return builder;
    }

    public static IDistributedApplicationBuilder WithHorizontallyScaledApplication(
        this IDistributedApplicationBuilder builder)
    {
        var db = builder
            .AddPostgres("database")
            .WithLifetime(ContainerLifetime.Persistent)
            .AddDatabase("plantbasedpizza");

        var instanceOne = builder.AddProject<Projects.PlantBasedPizza_Api>("instance-one")
            .WithReference(db)
            .WithEnvironment("ConnectionStrings__RecipesPostgresConnection", db)
            .WithEnvironment("ConnectionStrings__OrderManagerPostgresConnection", db)
            .WithEnvironment("ConnectionStrings__KitchenPostgresConnection", db)
            .WithEnvironment("ConnectionStrings__DeliveryPostgresConnection", db)
            .WithHttpEndpoint(8081)
            .WaitFor(db);
        var instanceTwo = builder.AddProject<Projects.PlantBasedPizza_Api>("instance-two")
            .WithReference(db)
            .WithEnvironment("ConnectionStrings__RecipesPostgresConnection", db)
            .WithEnvironment("ConnectionStrings__OrderManagerPostgresConnection", db)
            .WithEnvironment("ConnectionStrings__KitchenPostgresConnection", db)
            .WithEnvironment("ConnectionStrings__DeliveryPostgresConnection", db)
            .WithHttpEndpoint(8082)
            .WaitFor(db);
        var instanceThree = builder.AddProject<Projects.PlantBasedPizza_Api>("instance-three")
            .WithReference(db)
            .WithEnvironment("ConnectionStrings__RecipesPostgresConnection", db)
            .WithEnvironment("ConnectionStrings__OrderManagerPostgresConnection", db)
            .WithEnvironment("ConnectionStrings__KitchenPostgresConnection", db)
            .WithEnvironment("ConnectionStrings__DeliveryPostgresConnection", db)
            .WithHttpEndpoint(8083)
            .WaitFor(db);

        var yarpProxy = builder.AddYarp("gateway")
            .WithConfigFile("yarp.horizontal.json")
            .WithReference(instanceOne)
            .WithReference(instanceTwo)
            .WithReference(instanceThree);

        return builder;
    }


    public static IDistributedApplicationBuilder WithMigrationReadyApplications(
        this IDistributedApplicationBuilder builder)
    {
        var kafka = builder.AddKafka("kafka")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithKafkaUI()
            .WithLifetime(ContainerLifetime.Persistent);

        var recipesDb = builder
            .AddPostgres("recipesDb")
            .WithLifetime(ContainerLifetime.Persistent)
            .AddDatabase("plantbasedpizza-recipes");
        var recipeApplication = builder.AddProject<Projects.PlantBasedPizza_Recipes_Api>("recipe-api")
            .WithReference(recipesDb)
            .WithReference(kafka)
            .WithEnvironment("Messaging__Kafka", kafka)
            .WithEnvironment("ConnectionStrings__RecipesPostgresConnection", recipesDb)
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
            .WithEnvironment("Messaging__Kafka", kafka)
            .WithEnvironment("Services__RecipeApi", "http://recipe-api")
            .WithEnvironment("UseDistributedServices", "true")
            .WithEnvironment("ConnectionStrings__OrderManagerPostgresConnection", orderDb)
            .WithHttpEndpoint(8081)
            .WaitFor(orderDb)
            .WaitFor(kafka);
        var orderManagerWorker = builder.AddProject<Projects.PlantBasedPizza_Worker>("order-manager-worker")
            .WithReference(orderDb)
            .WithReference(recipeApplication)
            .WithReference(kafka)
            .WithEnvironment("Messaging__Kafka", kafka)
            .WithEnvironment("Services__RecipeApi", "http://recipe-api")
            .WithEnvironment("UseDistributedServices", "true")
            .WithEnvironment("ConnectionStrings__OrderManagerPostgresConnection", orderDb)
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
            .WithEnvironment("Messaging__Kafka", kafka)
            .WithEnvironment("Services__RecipeApi", "http://recipe-api")
            .WithEnvironment("Services__OrderApi", "http://order-manager-api")
            .WithEnvironment("UseDistributedServices", "true")
            .WithEnvironment("ConnectionStrings__KitchenPostgresConnection", kitchenDb)
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
            .WithEnvironment("ConnectionStrings__DeliveryPostgresConnection", deliveryDb)
            .WithHttpEndpoint(8083)
            .WaitFor(deliveryDb)
            .WaitFor(kafka);

        var yarpProxy = builder.AddYarp("gateway")
            .WithConfigFile("yarp.migration.json")
            .WithReference(orderManagerApplication)
            .WithReference(kitchenApplication)
            .WithReference(recipeApplication)
            .WithReference(deliveryApplication);

        return builder;
    }
}