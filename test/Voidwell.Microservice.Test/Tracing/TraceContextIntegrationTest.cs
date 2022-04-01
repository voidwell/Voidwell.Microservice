using FluentAssertions;
using Voidwell.Microservice.Tracing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Xunit;

namespace Voidwell.Microservice.Test.Tracing
{
    public class TraceContextIntegrationTest
    {
        [Fact]
        public async Task MultipleRequests_TraceContextAccessor_UniqueContextPerRequest()
        {
            int runCount = 10;
            ConcurrentBag<string> requestsRoots = new ConcurrentBag<string>();

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(a => a.AddTracing())
                .Configure(appBuilder =>
                {
                    appBuilder.UseTracing();

                    appBuilder.Run(ctx =>
                    {
                        var contextAccessor = ctx.RequestServices.GetRequiredService<ITraceContextAccessor>();
                        requestsRoots.Add(contextAccessor.TraceContext.Data.Root);
                        return Task.CompletedTask;
                    });
                });

            using (var server = new TestServer(webHostBuilder))
            using (var client = server.CreateClient())
            {
                var tasks = Enumerable.Range(0, runCount)
                    .Select(a => client.GetAsync("/"));

                await Task.WhenAll(tasks);
            }

            requestsRoots.Should().HaveCount(runCount, "that is the number of runs")
                .And.OnlyHaveUniqueItems("each request id is unique");
        }

        [Fact]
        public async Task MultipleRequests_TraceContextAccessor_SameRootWithinRequest()
        {
            int runCount = 10;
            ConcurrentBag<string> requestsRoots = new ConcurrentBag<string>();

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(a => a.AddTracing())
                .Configure(appBuilder =>
                {
                    appBuilder.UseTracing();

                    appBuilder.Run(ctx =>
                    {
                        Enumerable.Range(0, runCount)
                            .ToList()
                            .ForEach(a =>
                            {
                                var contextAccessor = ctx.RequestServices.GetRequiredService<ITraceContextAccessor>();
                                requestsRoots.Add(contextAccessor.TraceContext.Data.Root);
                            });

                        return Task.CompletedTask;
                    });
                });

            using (var server = new TestServer(webHostBuilder))
            using (var client = server.CreateClient())
            {
                await client.GetAsync("/");
            }

            requestsRoots.Should().HaveCount(runCount, "that is the number of runs")
                .And.OnlyContain(a => a == requestsRoots.First(), "each value should be equal");
        }

        [Fact]
        public async Task MultipleRequests_TraceContextAccessorViaSingleton_UniqueContextPerRequest()
        {
            int runCount = 10;
            ConcurrentBag<string> requestsRoots = new ConcurrentBag<string>();

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(a =>
                {
                    a.AddTracing();
                    a.AddSingleton<SingletonServiceWithTracing>();
                })
                .Configure(appBuilder =>
                {
                    appBuilder.UseTracing();

                    appBuilder.Run(ctx =>
                    {
                        var svc = ctx.RequestServices.GetRequiredService<SingletonServiceWithTracing>();
                        requestsRoots.Add(svc.GetRoot());
                        return Task.CompletedTask;
                    });
                });

            using (var server = new TestServer(webHostBuilder))
            using (var client = server.CreateClient())
            {
                var tasks = Enumerable.Range(0, runCount)
                    .Select(a => client.GetAsync("/"));

                await Task.WhenAll(tasks);
            }

            requestsRoots.Should().HaveCount(runCount, "that is the number of runs")
                .And.OnlyHaveUniqueItems("each request id is unique");
        }

        [Fact]
        public async Task MultipleRequests_TraceContextAccessorViaSingleton_SameRootWithinRequest()
        {
            int runCount = 10;
            ConcurrentBag<string> requestsRoots = new ConcurrentBag<string>();

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(a =>
                {
                    a.AddTracing();
                    a.AddSingleton<SingletonServiceWithTracing>();
                })
                .Configure(appBuilder =>
                {
                    appBuilder.UseTracing();

                    appBuilder.Run(ctx =>
                    {
                        Enumerable.Range(0, runCount)
                            .ToList()
                            .ForEach(a =>
                            {
                                var svc = ctx.RequestServices.GetRequiredService<SingletonServiceWithTracing>();
                                requestsRoots.Add(svc.GetRoot());
                            });

                        return Task.CompletedTask;
                    });
                });

            using (var server = new TestServer(webHostBuilder))
            using (var client = server.CreateClient())
            {
                await client.GetAsync("/");
            }

            requestsRoots.Should().HaveCount(runCount, "that is the number of runs")
                .And.OnlyContain(a => a == requestsRoots.First(), "each value should be equal");
        }

        [Fact]
        public async Task MultipleKnownRequests_TraceContextAccessor_RootCapturedInRequest()
        {
            var expectedRoots = Enumerable.Range(0, 10)
                .Select(a => Guid.NewGuid().ToString())
                .ToList();
            ConcurrentBag<string> requestsRoots = new ConcurrentBag<string>();

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(a => a.AddTracing())
                .Configure(appBuilder =>
                {
                    appBuilder.UseTracing();

                    appBuilder.Run(ctx =>
                    {
                        var contextAccessor = ctx.RequestServices.GetRequiredService<ITraceContextAccessor>();
                        requestsRoots.Add(contextAccessor.TraceContext.Data.Root);
                        return Task.CompletedTask;
                    });
                });

            using (var server = new TestServer(webHostBuilder))
            using (var client = server.CreateClient())
            {
                var requestTasks = expectedRoots
                        .Select(a =>
                        {
                            var msg = new HttpRequestMessage();
                            msg.Headers.Add(TraceContext.VoidwellTraceId, $"Root={a}");
                            return client.SendAsync(msg);
                        });

                await Task.WhenAll(requestTasks);
            }

            requestsRoots.Should().NotBeEmpty("requests were written to it")
                .And.BeEquivalentTo(expectedRoots, "these are the requests");
        }

        [Fact]
        public async Task RequestWithoutAncestry_RequestNoHeader_DefaultAncestry()
        {
            string ancestryResult = null;

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(a => a.AddTracing())
                .Configure(appBuilder =>
                {
                    appBuilder.UseTracing();

                    appBuilder.Run(ctx =>
                    {
                        var contextAccessor = ctx.RequestServices.GetRequiredService<ITraceContextAccessor>();
                        ancestryResult = contextAccessor.TraceContext.Data.Ancestry;
                        return Task.CompletedTask;
                    });
                });

            using (var server = new TestServer(webHostBuilder))
            using (var client = server.CreateClient())
            {
                await client.GetAsync("/");
            }

            ancestryResult.Should().NotBeNull("it should be set by default")
                .And.Be(TraceContext.DefaultAncestry, "this is the default value");
        }

        [Fact]
        public async Task RequestWithAncestry_AncestryInHeader_AncestryRead()
        {
            string expectedAncestry = "X:4:1";
            string ancestryResult = null;

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(a => a.AddTracing())
                .Configure(appBuilder =>
                {
                    appBuilder.UseTracing();

                    appBuilder.Run(ctx =>
                    {
                        var contextAccessor = ctx.RequestServices.GetRequiredService<ITraceContextAccessor>();
                        ancestryResult = contextAccessor.TraceContext.Data.Ancestry;
                        return Task.CompletedTask;
                    });
                });

            using (var server = new TestServer(webHostBuilder))
            using (var client = server.CreateClient())
            {
                var msg = new HttpRequestMessage();
                msg.Headers.Add(TraceContext.VoidwellTraceId, $"Root={Guid.NewGuid()};Ancestry={expectedAncestry}");
                await client.SendAsync(msg);
            }

            ancestryResult.Should().NotBeNull("it was set by the request")
                .And.Be(expectedAncestry, "that was the value in the header");
        }
    }
}
