using System.Text.Json;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    public class HttpRecipeService : IRecipeService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public HttpRecipeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<Recipe> GetRecipe(string recipeIdentifier)
        {
            var recipe = await this._httpClient.GetAsync($"/recipes/{recipeIdentifier}");

            return JsonSerializer.Deserialize<Recipe>(await recipe.Content.ReadAsStringAsync(), _jsonSerializerOptions);
        }
    }
}