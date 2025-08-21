using System.Text.Json;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Services;

namespace PlantBasedPizza.Kitchen.Infrastructure
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

        public async Task<RecipeAdapter> GetRecipe(string recipeIdentifier)
        {
            var recipe = await this._httpClient.GetAsync($"/recipes/{recipeIdentifier}");

            return JsonSerializer.Deserialize<RecipeAdapter>(await recipe.Content.ReadAsStringAsync(), _jsonSerializerOptions);
        }
    }
}