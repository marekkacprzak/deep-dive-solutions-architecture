using System.Text.Json;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Recipes.DataTransfer;
using Recipe = PlantBasedPizza.OrderManager.Core.Services.Recipe;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    public class HttpRecipeService : IRecipeService
    {
        private readonly HttpClient _httpClient;

        public HttpRecipeService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<Recipe> GetRecipe(string recipeIdentifier)
        {
            var recipe = await this._httpClient.GetAsync($"/recipes/{recipeIdentifier}");

            return JsonSerializer.Deserialize<Recipe>(await recipe.Content.ReadAsStringAsync());
        }
    }
}