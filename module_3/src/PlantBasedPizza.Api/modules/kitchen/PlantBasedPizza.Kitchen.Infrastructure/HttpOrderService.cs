using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Services;

namespace PlantBasedPizza.Kitchen.Infrastructure
{
    public class HttpOrderService : IOrderManagerService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly IDistributedCache _distributedCache;

        public HttpOrderService(HttpClient httpClient, IDistributedCache distributedCache)
        {
            _httpClient = httpClient;
            _distributedCache = distributedCache;
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };
        }

        public async Task<OrderAdapter> GetOrderDetails(string orderIdentifier)
        {
            var orderFromCache = await _distributedCache.GetStringAsync($"order:{orderIdentifier}");
            
            if (!string.IsNullOrEmpty(orderFromCache))
            {
                return JsonSerializer.Deserialize<OrderAdapter>(orderFromCache, _jsonSerializerOptions);
            }
            
            var httpResponse = await this._httpClient.GetAsync($"order/{orderIdentifier}/detail");

            var responseBody = await httpResponse.Content.ReadAsStringAsync();

            var orderAdapter = JsonSerializer.Deserialize<OrderAdapter>(responseBody);

            return orderAdapter;
        }
    }
}