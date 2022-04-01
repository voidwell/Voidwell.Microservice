using FluentAssertions;
using Voidwell.Microservice.Tracing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;
using IdentityModel.Client;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Voidwell.Microservice.Authentication;

namespace Voidwell.Microservice.Test.Tracing
{
    public class IdentityServerOptionsTracingSetupTest : IClassFixture<IdentityServerOptionsTracingSetupFixture>
    {
        private readonly IdentityServerOptionsTracingSetupFixture _fixture;

        public IdentityServerOptionsTracingSetupTest(IdentityServerOptionsTracingSetupFixture fixture)
        {
            _fixture = fixture;
            _fixture.ResetFixture();
        }

        [Theory]
        [InlineData(TestIdentityProvider.ClientWithJtw)]
        [InlineData(TestIdentityProvider.ClientWithReference)]
        public async Task AddTracingToAccessTokenValidation_Valid_RequestsHaveTraceId(string clientId)
        {
            var accessToken = await _fixture.IdentityProvider.GetTokenAsync(clientId);

            bool wasCalled = false;
            bool hasKey = false;

            _fixture.WrappedMessageHandlerAction = httpRequestMessage =>
            {
                wasCalled = true;
                hasKey = httpRequestMessage.Headers.Contains(TraceContext.VoidwellTraceId);
                return Task.CompletedTask;
            };

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices((ctx, services) =>
                {
                    services.AddTracing();

                    services.AddAuthentication("token")
                        .AddServiceAuthentication("token", options =>
                        {
                            options.Authority = _fixture.IdentityProvider.BaseAddress;
                            options.SupportedTokens = SupportedTokens.Both;
                            options.Audience = TestIdentityProvider.Api1Name;
                            options.ClientId = TestIdentityProvider.Api1Name;
                            options.ClientSecret = TestIdentityProvider.Api1Secret;
                            options.BackchannelHttpHandler = _fixture.WrappedMessageHandler;

                            options.RequireHttpsMetadata = false;
                        });

                    services.AddHttpClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName)
                        .AddHttpMessageHandler(() => _fixture.WrappedMessageHandler);
                })
                .Configure(app =>
                {
                    app.UseTracing();
                    app.UseAuthentication();

                    app.Run(async ctx =>
                    {
                        var result = await ctx.AuthenticateAsync();
                        result.Failure?.Should()
                            .BeNull();

                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        return;
                    });
                });

            using (var server = new TestServer(webHostBuilder))
            using (var client = server.CreateClient())
            {
                client.SetBearerToken(accessToken);

                var response = await client.GetAsync("");

                response.StatusCode.Should()
                    .Be(HttpStatusCode.OK);
            }

            wasCalled.Should()
                .BeTrue("did not call action");
            hasKey.Should()
                .BeTrue("did not have key");
        }
    }
}
