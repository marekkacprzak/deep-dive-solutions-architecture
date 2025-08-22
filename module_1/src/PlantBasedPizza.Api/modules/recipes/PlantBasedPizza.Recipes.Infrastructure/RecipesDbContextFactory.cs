



using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PlantBasedPizza.Recipes.Infrastructure;

internal class RecipesDbContextFactory : IDesignTimeDbContextFactory<RecipesDbContext>
{
    public RecipesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RecipesDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=PlantBasedPizza_Recipes;Username=postgres;Password=yourpassword");

        return new RecipesDbContext(optionsBuilder.Options);
    }
}