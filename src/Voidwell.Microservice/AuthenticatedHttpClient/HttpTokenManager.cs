using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Voidwell.Microservice.AuthenticatedHttpClient
{
    public class HttpTokenManager : IHttpTokenManager, IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<AuthenticatedHttpClientOptions> _optionsMonitor;
        private readonly ILogger<HttpTokenManager> _logger;

        private readonly Dictionary<string, TokenState> _tokens = new Dictionary<string, TokenState>();
        private readonly SemaphoreSlim _tokenSemaphore = new SemaphoreSlim(1);

        public HttpTokenManager(IHttpClientFactory httpClientFactory, IOptionsMonitor<AuthenticatedHttpClientOptions> optionsMonitor, ILogger<HttpTokenManager> logger)
        {
            _httpClientFactory = httpClientFactory;
            _optionsMonitor = optionsMonitor;
            _logger = logger;
        }

        public async Task<string> GetTokenAsync(string clientName)
        {
            if (!IsTokenValid(clientName))
            {
                await UpdateTokenAsync(clientName);
            }

            return _tokens[clientName]?.AccessToken;
        }

        private async Task UpdateTokenAsync(string clientName)
        {
            await _tokenSemaphore.WaitAsync();

            if (IsTokenValid(clientName))
            {
                return;
            }

            try
            {
                _tokens.Remove(clientName);

                var response = await RequestNewToken(clientName);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var tokenNode = JsonNode.Parse(responseString);
                var accessToken = tokenNode["access_token"].GetValue<string>();
                var expiresIn = tokenNode["expires_in"].GetValue<int>();


                _tokens[clientName] = new TokenState(accessToken, expiresIn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retreive new token for '{ClientName}'", clientName);
            }
            finally
            {
                _tokenSemaphore.Release();
            }
        }

        private Task<HttpResponseMessage> RequestNewToken(string clientName)
        {
            var clientOptions = _optionsMonitor.Get(clientName);

            var payload = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", clientOptions.ClientId },
                    { "client_secret", clientOptions.ClientSecret },
                    { "scope", string.Join(" ", clientOptions.Scopes) }
                };

            var httpClient = _httpClientFactory.CreateClient(Constants.DefaultTraceHttpClient);
            return httpClient.PostAsync(clientOptions.TokenServiceAddress, new FormUrlEncodedContent(payload));
        }

        private bool IsTokenValid(string clientName)
        {
            return _tokens.ContainsKey(clientName) && !_tokens[clientName].IsExpired();
        }

        public void Dispose()
        {
            _tokenSemaphore.Dispose();
        }
    }
}
