using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using IntegrationTests.ViewModels;

namespace IntegrationTests.Drivers
{
    public class ApplicationDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public ApplicationDriver()
        {
            this._httpClient = new HttpClient();
        }

        public async Task<Entity> DoThing(string orderIdentifier)
        {
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/order/{orderIdentifier}/detail"))
                .ConfigureAwait(false);

            var order = JsonSerializer.Deserialize<Entity>(await result.Content.ReadAsStringAsync(),_jsonSerializerOptions);

            return order;
        }
    }
}