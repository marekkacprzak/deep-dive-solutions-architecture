using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Services;

namespace PlantBasedPizza.Kitchen.Infrastructure
{
    public class HttpRecipeService(HttpClient httpClient, IDistributedCache distributedCache, ActivitySource activitySource) : IRecipeService
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        private readonly IDistributedCache _distributedCache = distributedCache;

        public async Task<RecipeAdapter> GetRecipe(string recipeIdentifier)
        {
            using var getRecipeActivity = activitySource.StartActivity("get-recipe");
            getRecipeActivity?.SetTag("recipeIdentifier", recipeIdentifier);
            
            var cachedRecipe = await _distributedCache.GetStringAsync($"kitchen:recipe:{recipeIdentifier}");

            if (!string.IsNullOrEmpty(cachedRecipe))
            {
                getRecipeActivity?.SetTag("cache.hit", "true");
                return JsonSerializer.Deserialize<RecipeAdapter>(cachedRecipe, _jsonSerializerOptions);
            }
            
            getRecipeActivity?.SetTag("cache.hit", "false");
            
            var recipe = await httpClient.GetAsync($"/recipes/{recipeIdentifier}");
            
            await _distributedCache.SetStringAsync($"kitchen:recipe:{recipeIdentifier}",
                await recipe.Content.ReadAsStringAsync(),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });

            return JsonSerializer.Deserialize<RecipeAdapter>(await recipe.Content.ReadAsStringAsync(), _jsonSerializerOptions);
        }
    }
}