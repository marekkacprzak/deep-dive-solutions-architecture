using PlantBasedPizza.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

builder.WithDistributedServices();

await builder.Build().RunAsync();