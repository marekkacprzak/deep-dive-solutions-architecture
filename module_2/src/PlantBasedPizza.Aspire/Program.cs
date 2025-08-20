using PlantBasedPizza.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

builder.WithSingleApplicationInstance();
// builder.WithHorizontallyScaledApplication();
//builder.WithMigrationReadyApplications();

await builder.Build().RunAsync();