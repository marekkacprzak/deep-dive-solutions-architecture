var builder = DistributedApplication.CreateBuilder(args);

var db = builder
    .AddPostgres("database")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("plantbasedpizza");

var orderManagerApplication = builder.AddProject<Projects.PlantBasedPizza_Api>("order-manager-api")
    .WithReference(db)
    .WithEnvironment("ConnectionStrings__RecipesPostgresConnection", db)
    .WithEnvironment("ConnectionStrings__OrderManagerPostgresConnection", db)
    .WithEnvironment("ConnectionStrings__KitchenPostgresConnection", db)
    .WithEnvironment("ConnectionStrings__DeliveryPostgresConnection", db)
    .WithHttpEndpoint(8081)
    .WaitFor(db);

var kitchenApplication = builder.AddProject<Projects.PlantBasedPizza_Api>("kitchen-api")
    .WithReference(db)
    .WithEnvironment("ConnectionStrings__RecipesPostgresConnection", db)
    .WithEnvironment("ConnectionStrings__OrderManagerPostgresConnection", db)
    .WithEnvironment("ConnectionStrings__KitchenPostgresConnection", db)
    .WithEnvironment("ConnectionStrings__DeliveryPostgresConnection", db)
    .WithHttpEndpoint(8082)
    .WaitFor(db);

var deliveryApplication = builder.AddProject<Projects.PlantBasedPizza_Api>("delivery-api")
    .WithReference(db)
    .WithEnvironment("ConnectionStrings__RecipesPostgresConnection", db)
    .WithEnvironment("ConnectionStrings__OrderManagerPostgresConnection", db)
    .WithEnvironment("ConnectionStrings__KitchenPostgresConnection", db)
    .WithEnvironment("ConnectionStrings__DeliveryPostgresConnection", db)
    .WithHttpEndpoint(8083)
    .WaitFor(db);

var recipeApplication = builder.AddProject<Projects.PlantBasedPizza_Api>("recipe-api")
    .WithReference(db)
    .WithEnvironment("ConnectionStrings__RecipesPostgresConnection", db)
    .WithEnvironment("ConnectionStrings__OrderManagerPostgresConnection", db)
    .WithEnvironment("ConnectionStrings__KitchenPostgresConnection", db)
    .WithEnvironment("ConnectionStrings__DeliveryPostgresConnection", db)
    .WithHttpEndpoint(8084)
    .WaitFor(db);

var yarpProxy = builder.AddYarp("gateway")
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("/order/{**catch-all}", orderManagerApplication);
        yarp.AddRoute("/kitchen/{**catch-all}", kitchenApplication);
        yarp.AddRoute("/delivery/{**catch-all}", deliveryApplication);
        yarp.AddRoute("/recipes/{**catch-all}", recipeApplication);
        yarp.AddRoute("/utils/__migrate", orderManagerApplication);
    })
    .WithHostPort(8080);

builder.Build().Run();