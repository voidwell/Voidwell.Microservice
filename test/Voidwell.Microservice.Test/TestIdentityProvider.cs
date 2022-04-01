using IdentityModel.Client;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using IdSvr = IdentityServer4.Models;

namespace Voidwell.Microservice.Test
{
    public class TestIdentityProvider : IDisposable
    {
        private readonly TestServer _server;

        public const string Api1Name = "api1";
        public const string Api1Secret = "apisecret";
        public const string ClientWithJtw = "client_jwt";
        public const string ClientWithReference = "client_reference";
        public const string ClientSecret = "clientsecret";

        public TestIdentityProvider()
        {
            var builder = new WebHostBuilder()
                .UseStartup<TestIdentityProviderStartup>();

            _server = new TestServer(builder);
        }

        public async Task<string> GetTokenAsync(string clientId)
        {
            var client = new TokenClient(
                new HttpMessageInvoker(CreateMessageHandler()),
                new TokenClientOptions {
                    Address = TokenServiceAddress,
                    ClientId = clientId,
                    ClientSecret = ClientSecret
                });

            var response = await client.RequestClientCredentialsTokenAsync(Api1Name);

            if (response.IsError || string.IsNullOrEmpty(response.AccessToken))
            {
                throw new InvalidOperationException($"Error when getting token: {response.Error}");
            }

            return response.AccessToken;
        }

        public void Dispose()
        {
            _server.Dispose();
        }

        public HttpMessageHandler CreateMessageHandler() => _server.CreateHandler();

        public string BaseAddress => _server.BaseAddress.ToString();

        public string TokenIntrospectionAddress => new UriBuilder(_server.BaseAddress)
        {
            Path = PathString.FromUriComponent(_server.BaseAddress).Add("/connect/introspect")
        }.ToString();

        public string TokenServiceAddress => new UriBuilder(_server.BaseAddress)
            {
                Path = PathString.FromUriComponent(_server.BaseAddress).Add("/connect/token")
            }.ToString();

        private class TestIdentityProviderStartup
        {
            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                services.AddIdentityServer(o => o.Endpoints.EnableDiscoveryEndpoint = true)
                    .AddInMemoryClients(GetClients())
                    .AddInMemoryApiResources(GetApiResources())
                    .AddInMemoryApiScopes(GetApiScopes())
                    .AddInMemoryPersistedGrants()
                    .AddDeveloperSigningCredential();

                return services.BuildServiceProvider();
            }

            public void Configure(IApplicationBuilder app)
            {
                app.UseIdentityServer();
            }
        }

        private static IEnumerable<IdSvr.Client> GetClients()
        {
            yield return new IdSvr.Client
            {
                ClientId = ClientWithJtw,
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets =
                {
                    new IdSvr.Secret(ClientSecret.Sha256())
                },
                AllowedScopes =
                {
                    Api1Name
                },
                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = 3600
            };

            yield return new IdSvr.Client
            {
                ClientId = ClientWithReference,
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets =
                {
                    new IdSvr.Secret(ClientSecret.Sha256())
                },
                AllowedScopes =
                {
                    Api1Name
                },
                AccessTokenType = AccessTokenType.Reference,
                AccessTokenLifetime = 3600
            };
        }

        private static IEnumerable<ApiResource> GetApiResources()
        {
            yield return new ApiResource(Api1Name)
            {
                ApiSecrets = { new IdSvr.Secret(Api1Secret.Sha256()) },
                Scopes =
                {
                    Api1Name
                }
            };
        }

        private static IEnumerable<IdSvr.ApiScope> GetApiScopes()
        {
            yield return new IdSvr.ApiScope(Api1Name);
        }
    }
}
