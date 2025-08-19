using System.Text.Json;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Kitchen.Infrastructure
{
    public class HttpOrderService : IOrderManagerService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public HttpOrderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };
        }

        public async Task<OrderAdapter> GetOrderDetails(string orderIdentifier)
        {
            var httpResponse = await this._httpClient.GetAsync($"order/{orderIdentifier}/detail");

            var responseBody = await httpResponse.Content.ReadAsStringAsync();

            var orderAdapter = JsonSerializer.Deserialize<OrderAdapter>(responseBody);

            return orderAdapter;
        }
    }
}