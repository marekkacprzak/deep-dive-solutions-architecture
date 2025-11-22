using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Infrastructure;

internal class RecipeRepositoryPostgres(
    RecipesDbContext context,
    ILogger<RecipeRepositoryPostgres> logger,
    IDistributedCache cache)
    : IRecipeRepository
{
    private readonly ILogger<RecipeRepositoryPostgres> _logger = logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
    };

    private const string RecipeListCacheKey = "recipe:list";

    public async Task<Recipe?> Retrieve(string recipeIdentifier)
    {
        var cacheKey = $"recipe:{recipeIdentifier}";
        
        var cachedRecipe = await cache.GetAsync(cacheKey);
        
        if (cachedRecipe != null)
        {
            Activity.Current?.AddTag("cache.hit", true);
            return JsonSerializer.Deserialize<Recipe>(cachedRecipe, _jsonSerializerOptions);
        }
        
        Activity.Current?.AddTag("cache.hit", false);
        
        var recipe = await context.Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.RecipeIdentifier == recipeIdentifier);

        if (recipe != null)
        {
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(recipe), new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(15)
            });
        }
        
        return recipe;
    }

    public async Task<IEnumerable<Recipe>> List()
    {
        var recipeListCache = await cache.GetStringAsync(RecipeListCacheKey);
        
        if (recipeListCache != null)
        {
            Activity.Current?.AddTag("cache.hit", true);
            return JsonSerializer.Deserialize<IEnumerable<Recipe>>(recipeListCache, _jsonSerializerOptions) ?? Enumerable.Empty<Recipe>();
        }
        
        Activity.Current?.AddTag("cache.hit", false);
        
        var recipes = await context
            .Recipes
            .Include(r => r.Ingredients)
            .ToListAsync();

        if (recipes.Any())
        {
            await cache.SetStringAsync(RecipeListCacheKey, JsonSerializer.Serialize(recipes), new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(15)
            });
        }
        
        return recipes;
    }

    public async Task Add(Recipe? recipe)
    {
        await context.Recipes.AddAsync(recipe!);
        await context.SaveChangesAsync();
        
        await cache.RemoveAsync(RecipeListCacheKey);
    }

    public async Task Update(Recipe? recipe)
    {
        context.Recipes.Update(recipe!);
        await context.SaveChangesAsync();
        
        await cache.RemoveAsync(RecipeListCacheKey);
        await cache.RemoveAsync($"recipe:{recipe?.RecipeIdentifier}");
    }
} 