



using Microsoft.EntityFrameworkCore;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Infrastructure;

public class RecipesDbContext : DbContext
{
    public RecipesDbContext(DbContextOptions<RecipesDbContext> options) : base(options)
    {
    }

    public DbSet<Recipe> Recipes { get; set; }
    
    public DbSet<Ingredient> Ingredients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>()
            .HasKey(r => r.RecipeIdentifier);
        
        modelBuilder.Entity<Recipe>()
            .ToTable("recipes")
            .HasMany(i => i.Ingredients)
            .WithOne()
            .HasForeignKey("RecipeIdentifier")
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Ingredient>()
            .HasKey(i => i.IngredientIdentifier);
    }
}