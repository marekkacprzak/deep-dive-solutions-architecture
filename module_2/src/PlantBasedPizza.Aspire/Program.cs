using PlantBasedPizza.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

// builder.WithSingleApplicationInstance();
// builder.WithHorizontallyScaledApplicaton();
builder.WithMigrationReadyApplications();

await builder.Build().RunAsync();