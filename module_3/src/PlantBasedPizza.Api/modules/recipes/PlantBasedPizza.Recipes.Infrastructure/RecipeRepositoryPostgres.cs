using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Infrastructure;

internal class RecipeRepositoryPostgres : IRecipeRepository
{
    private readonly ILogger<RecipeRepositoryPostgres> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
    };
    
    private readonly RecipesDbContext _context;
    private readonly IDistributedCache _cache;
    private const string RecipeListCacheKey = "recipe:list";

    public RecipeRepositoryPostgres(RecipesDbContext context, ILogger<RecipeRepositoryPostgres> logger, IDistributedCache cache)
    {
        _logger = logger;
        _cache = cache;
        _context = context;
    }
    
    public async Task<Recipe?> Retrieve(string recipeIdentifier)
    {
        var cacheKey = $"recipe:{recipeIdentifier}";
        
        var cachedRecipe = await _cache.GetAsync(cacheKey);
        
        if (cachedRecipe != null)
        {
            Activity.Current?.AddTag("cache.hit", true);
            return System.Text.Json.JsonSerializer.Deserialize<Recipe>(cachedRecipe, _jsonSerializerOptions);
        }
        
        Activity.Current?.AddTag("cache.hit", false);
        
        var recipe = await _context.Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.RecipeIdentifier == recipeIdentifier);

        if (recipe != null)
        {
            await _cache.SetStringAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(recipe), new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(15)
            });
        }
        
        return recipe;
    }

    public async Task<IEnumerable<Recipe>> List()
    {
        var recipeListCache = await _cache.GetStringAsync(RecipeListCacheKey);
        
        if (recipeListCache != null)
        {
            Activity.Current?.AddTag("cache.hit", true);
            return JsonSerializer.Deserialize<IEnumerable<Recipe>>(recipeListCache, _jsonSerializerOptions) ?? Enumerable.Empty<Recipe>();
        }
        
        Activity.Current?.AddTag("cache.hit", false);
        
        var recipes = await _context
            .Recipes
            .Include(r => r.Ingredients)
            .ToListAsync();

        if (recipes.Any())
        {
            await _cache.SetStringAsync(RecipeListCacheKey, System.Text.Json.JsonSerializer.Serialize(recipes), new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(15)
            });
        }
        
        return recipes;
    }

    public async Task Add(Recipe? recipe)
    {
        await _context.Recipes.AddAsync(recipe);
        await _context.SaveChangesAsync();
        
        await _cache.RemoveAsync(RecipeListCacheKey);
    }

    public async Task Update(Recipe? recipe)
    {
        _context.Recipes.Update(recipe);
        await _context.SaveChangesAsync();
        
        await _cache.RemoveAsync(RecipeListCacheKey);
        await _cache.RemoveAsync($"recipe:{recipe?.RecipeIdentifier}");
    }
} 