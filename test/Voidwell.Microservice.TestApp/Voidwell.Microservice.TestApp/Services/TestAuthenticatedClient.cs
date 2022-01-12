using System.Text.Json.Nodes;
using Voidwell.Microservice.Extensions;

namespace Voidwell.Microservice.TestApp.Services
{
    public class TestAuthenticatedClient : ITestAuthenticatedClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TestAuthenticatedClient> _logger;
        
        public TestAuthenticatedClient(HttpClient httpClient, ILogger<TestAuthenticatedClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<JsonNode> TestAsync(string characterName)
        {
            var result = await _httpClient.GetAsync("https://api.voidwell.com/ps2/character/byname/Lampjaw");

            _logger.LogInformation(1234, "Retrieved character: {Name}", characterName);

            return await result.GetContentAsync<JsonNode>();
        }
    }
}
