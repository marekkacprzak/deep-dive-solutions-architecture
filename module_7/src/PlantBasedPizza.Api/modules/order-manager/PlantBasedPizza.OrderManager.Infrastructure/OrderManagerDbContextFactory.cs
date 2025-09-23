



using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PlantBasedPizza.OrderManager.Infrastructure;

internal class OrderManagerDbContextFactory : IDesignTimeDbContextFactory<OrderManagerDbContext>
{
    public OrderManagerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderManagerDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=PlantBasedPizza_Orders;Username=postgres;Password=yourpassword");

        return new OrderManagerDbContext(optionsBuilder.Options);
    }
}