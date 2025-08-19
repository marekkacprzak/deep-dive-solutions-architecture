using System.Text.Json;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Kitchen.Infrastructure
{
    public class HttpRecipeService : IRecipeService
    {
        private readonly HttpClient _httpClient;

        public HttpRecipeService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<RecipeAdapter> GetRecipe(string recipeIdentifier)
        {
            var recipe = await this._httpClient.GetAsync($"/recipes/{recipeIdentifier}");

            return JsonSerializer.Deserialize<RecipeAdapter>(await recipe.Content.ReadAsStringAsync());
        }
    }
}