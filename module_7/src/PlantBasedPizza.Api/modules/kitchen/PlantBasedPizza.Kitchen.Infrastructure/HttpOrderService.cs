using System.Diagnostics;
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
        private readonly ActivitySource _activitySource;

        public HttpOrderService(HttpClient httpClient, IDistributedCache distributedCache, ActivitySource activitySource)
        {
            _httpClient = httpClient;
            _distributedCache = distributedCache;
            _activitySource = activitySource;
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };
        }

        public async Task<OrderAdapter> GetOrderDetails(string orderIdentifier)
        {
            using var getOrderDetailsSpan = _activitySource.StartActivity("getOrderDetails");
            var orderFromCache = await _distributedCache.GetStringAsync($"order:{orderIdentifier}");
            
            if (!string.IsNullOrEmpty(orderFromCache))
            {
                getOrderDetailsSpan?.SetTag("cache.hit", "true");
                return JsonSerializer.Deserialize<OrderAdapter>(orderFromCache, _jsonSerializerOptions);
            }
            getOrderDetailsSpan?.SetTag("cache.hit", "false");
            
            var httpResponse = await this._httpClient.GetAsync($"order/{orderIdentifier}/detail");

            var responseBody = await httpResponse.Content.ReadAsStringAsync();

            var orderAdapter = JsonSerializer.Deserialize<OrderAdapter>(responseBody);

            return orderAdapter;
        }
    }
}