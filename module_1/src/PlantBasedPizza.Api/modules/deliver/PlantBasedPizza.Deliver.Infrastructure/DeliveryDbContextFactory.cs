using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PlantBasedPizza.Deliver.Infrastructure;

public class DeliveryDbContextFactory : IDesignTimeDbContextFactory<DeliveryDbContext>
{
    public DeliveryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DeliveryDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=PlantBasedPizza_Delivery;Username=postgres;Password=yourpassword");

        return new DeliveryDbContext(optionsBuilder.Options);
    }
}