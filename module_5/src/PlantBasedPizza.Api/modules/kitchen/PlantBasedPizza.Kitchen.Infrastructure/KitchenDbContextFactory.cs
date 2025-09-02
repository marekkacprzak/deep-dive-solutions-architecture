



using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PlantBasedPizza.Kitchen.Infrastructure;

public class KitchenDbContextFactory : IDesignTimeDbContextFactory<KitchenDbContext>
{
    public KitchenDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<KitchenDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=PlantBasedPizza_Kitchen;Username=postgres;Password=yourpassword");

        return new KitchenDbContext(optionsBuilder.Options);
    }
}