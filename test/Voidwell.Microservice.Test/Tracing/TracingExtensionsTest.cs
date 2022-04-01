using FluentAssertions;
using Voidwell.Microservice.Tracing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Voidwell.Microservice.Logging.Enrichers;

namespace Voidwell.Microservice.Test.Tracing
{
    public class TracingExtensionsTest
    {
        [Fact]
        public void AddTracing_TracingContextFactoryFromServiceProvider_NotNull()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(a => a.AddTracing())
                .Configure(appBuilder => { });

            using (var server = new TestServer(webHostBuilder))
            {
                var result = server.Host.Services.GetService<Func<TraceContext>>();
                result.Should().NotBeNull();
            }
        }

        [Fact]
        public void AddTracing_TraceContextAccesssorContextFromServiceProvider_NotNull()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(a => a.AddTracing())
                .Configure(appBuilder => { });

            using (var server = new TestServer(webHostBuilder))
            {
                var result = server.Host.Services.GetService<ITraceContextAccessor>();
                result.Should().NotBeNull()
                    .And.BeOfType<TraceContextAccessor>();
            }
        }

        [Fact]
        public void AddTracing_TracingHttpMessageHandlerFromServiceProvider_CorrectType()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(a => a.AddTracing())
                .Configure(appBuilder => { });

            using (var server = new TestServer(webHostBuilder))
            {
                var result = server.Host.Services.GetService<TracingHttpMessageHandler>();

                result.Should().NotBeNull()
                    .And.BeOfType<TracingHttpMessageHandler>();
            }
        }

        [Fact]
        public void AddTracing_TracingEnricherFromServiceProvider_CorrectType()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(a => a.AddTracing())
                .Configure(appBuilder => { });

            using (var server = new TestServer(webHostBuilder))
            {
                var result = server.Host.Services.GetService<TracingEnricher>();

                result.Should().NotBeNull()
                    .And.BeOfType<TracingEnricher>();
            }
        }
    }
}
