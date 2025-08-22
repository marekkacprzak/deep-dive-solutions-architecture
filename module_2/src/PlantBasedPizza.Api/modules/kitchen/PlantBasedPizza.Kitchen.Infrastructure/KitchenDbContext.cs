



using Microsoft.EntityFrameworkCore;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Entities;

namespace PlantBasedPizza.Kitchen.Infrastructure;

public class KitchenDbContext : DbContext
{
    public KitchenDbContext(DbContextOptions<KitchenDbContext> options) : base(options)
    {
    }

    public DbSet<KitchenRequest> KitchenRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KitchenRequest>()
            .HasKey(k => k.OrderIdentifier);
        
        modelBuilder.Entity<RecipeAdapter>()
            .HasKey(k => k.RecipeAdapterId);
        
        modelBuilder.Entity<KitchenRequest>()
            .ToTable("kitchen")
            .HasMany(k => k.Recipes)
            .WithOne()
            .HasForeignKey("KitchenRequestId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}