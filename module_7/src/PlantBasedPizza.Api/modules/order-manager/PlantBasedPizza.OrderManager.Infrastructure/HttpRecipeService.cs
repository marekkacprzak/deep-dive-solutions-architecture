using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    public class HttpRecipeService(HttpClient httpClient, IDistributedCache distributedCache, ActivitySource activitySource) : IRecipeService
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public async Task<Recipe> GetRecipe(string recipeIdentifier)
        {
            using var getRecipeActivity = activitySource.StartActivity("get-recipe");
            getRecipeActivity?.SetTag("recipeIdentifier", recipeIdentifier);
            
            var cachedRecipe = await distributedCache.GetStringAsync($"order-manager:recipe:{recipeIdentifier}");

            if (!string.IsNullOrEmpty(cachedRecipe))
            {
                getRecipeActivity?.SetTag("cache.hit", "true");
                return JsonSerializer.Deserialize<Recipe>(cachedRecipe, _jsonSerializerOptions) ?? throw new InvalidOperationException("Failed to deserialize recipe from cache");
            }
            
            getRecipeActivity?.SetTag("cache.hit", "false");
            
            var recipe = await httpClient.GetAsync($"/recipes/{recipeIdentifier}");
            
            await distributedCache.SetStringAsync($"kitchen:recipe:{recipeIdentifier}",
                await recipe.Content.ReadAsStringAsync(),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });

            return JsonSerializer.Deserialize<Recipe>(await recipe.Content.ReadAsStringAsync(), _jsonSerializerOptions) ?? throw new InvalidOperationException("Failed to deserialize recipe from response");
        }
    }
}