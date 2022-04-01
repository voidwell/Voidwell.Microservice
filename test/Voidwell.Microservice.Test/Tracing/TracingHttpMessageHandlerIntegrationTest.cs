using FluentAssertions;
using Voidwell.Microservice.Tracing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Net;
using Xunit;

namespace Voidwell.Microservice.Test.Tracing
{
    public class TracingHttpMessageHandlerIntegrationTest
    {
        [Fact]
        public async Task TracingHttpMessageHandler_MultipleCallsFromMultipleRoots_EachRootCaptured()
        {
            int runCount = 10;
            var expectedRoots = Enumerable.Range(0, runCount)
                .Select(a => Guid.NewGuid().ToString())
                .ToList();

            ConcurrentBag<string> rootsRecordedInServer2 = new ConcurrentBag<string>();

            var webHost2Builder = new WebHostBuilder()
                .ConfigureServices(a => a.AddTracing())
                .Configure(a =>
                {
                    a.UseTracing();

                    a.Run(ctx =>
                    {
                        var contextAccesssor = ctx.RequestServices.GetRequiredService<ITraceContextAccessor>();
                        rootsRecordedInServer2.Add(contextAccesssor.TraceContext.Data.Root);
                        return Task.CompletedTask;
                    });
                });

            using (var server2 = new TestServer(webHost2Builder))
            {
                var webHost1Builder = new WebHostBuilder()
                    .ConfigureServices(a => a.AddTracing())
                    .Configure(a =>
                    {
                        a.UseTracing();

                        var messageHandler = a.ApplicationServices.GetRequiredService<TracingHttpMessageHandler>();
                        messageHandler.InnerHandler = server2.CreateHandler();
                        var httpClient = new HttpClient(messageHandler) { BaseAddress = server2.BaseAddress };

                        a.Run(ctx => httpClient.GetAsync("/"));
                    });

                using (var server1 = new TestServer(webHost1Builder))
                using (var client1 = server1.CreateClient())
                {
                    var requestTasks = expectedRoots
                        .Select(a =>
                        {
                            var msg = new HttpRequestMessage();
                            msg.Headers.Add(TraceContext.VoidwellTraceId, $"Root={a}");
                            return client1.SendAsync(msg);
                        });

                    await Task.WhenAll(requestTasks);
                }

                rootsRecordedInServer2.Should().NotBeEmpty("requests were written to it")
                    .And.BeEquivalentTo(expectedRoots, "these are the requests");
            }
        }

        [Fact]
        public async Task TracingHttpMessageHandler_MultipleCallsFromWithinSameRequest_AncestryDataCapture()
        {
            int runCount = 10;
            var expectedAncestry = Enumerable.Range(0, runCount)
                .Select(a => $"X:{a}")
                .ToList();

            ConcurrentBag<string> ancestryRecordedInServer2 = new ConcurrentBag<string>();

            var webHost2Builder = new WebHostBuilder()
                .ConfigureServices(a => a.AddTracing())
                .Configure(a =>
                {
                    a.UseTracing();

                    a.Run(ctx =>
                    {
                        var contextAccesssor = ctx.RequestServices.GetRequiredService<ITraceContextAccessor>();
                        ancestryRecordedInServer2.Add(contextAccesssor.TraceContext.Data.Ancestry);
                        return Task.CompletedTask;
                    });
                });

            using (var server2 = new TestServer(webHost2Builder))
            {
                var webHost1Builder = new WebHostBuilder()
                    .ConfigureServices(a => a.AddTracing())
                    .Configure(a =>
                    {
                        a.UseTracing();

                        var messageHandler = a.ApplicationServices.GetRequiredService<TracingHttpMessageHandler>();
                        messageHandler.InnerHandler = server2.CreateHandler();
                        var httpClient = new HttpClient(messageHandler) { BaseAddress = server2.BaseAddress };

                        a.Run(ctx =>
                        {
                            var requestsFromServer1 = expectedAncestry
                                .Select(x => httpClient.GetAsync("/"))
                                .ToList();

                            return Task.WhenAll(requestsFromServer1);
                        });
                    });

                using (var server1 = new TestServer(webHost1Builder))
                using (var client1 = server1.CreateClient())
                {
                    await client1.GetAsync("/");
                }

                ancestryRecordedInServer2.Should().NotBeEmpty("requests were written to it")
                    .And.BeEquivalentTo(expectedAncestry, "these are the ancestryResults that should have appeared");
            }
        }

        [Fact]
        public async Task TracingHttpMessageHandler_MultipleCallsFromWithinSameRequestWithAncestry_AncestryDataCapture()
        {
            int runCount = 10;
            var originalAncestry = "X:4:2";
            var expectedAncestry = Enumerable.Range(0, runCount)
                .Select(a => $"{originalAncestry}:{a}")
                .ToList();

            ConcurrentBag<string> ancestryRecordedInServer2 = new ConcurrentBag<string>();

            var webHost2Builder = new WebHostBuilder()
                .ConfigureServices(a => a.AddTracing())
                .Configure(a =>
                {
                    a.UseTracing();

                    a.Run(ctx =>
                    {
                        var contextAccesssor = ctx.RequestServices.GetRequiredService<ITraceContextAccessor>();
                        ancestryRecordedInServer2.Add(contextAccesssor.TraceContext.Data.Ancestry);
                        return Task.CompletedTask;
                    });
                });

            using (var server2 = new TestServer(webHost2Builder))
            {
                var webHost1Builder = new WebHostBuilder()
                    .ConfigureServices(a => a.AddTracing())
                    .Configure(a =>
                    {
                        a.UseTracing();

                        var messageHandler = a.ApplicationServices.GetRequiredService<TracingHttpMessageHandler>();
                        messageHandler.InnerHandler = server2.CreateHandler();
                        var httpClient = new HttpClient(messageHandler) { BaseAddress = server2.BaseAddress };

                        a.Run(ctx =>
                        {
                            var requestsFromServer1 = expectedAncestry
                                .Select(x => httpClient.GetAsync("/"))
                                .ToList();

                            return Task.WhenAll(requestsFromServer1);
                        });
                    });

                using (var server1 = new TestServer(webHost1Builder))
                using (var client1 = server1.CreateClient())
                {
                    var msg = new HttpRequestMessage();
                    msg.Headers.Add(TraceContext.VoidwellTraceId, $"Root={Guid.NewGuid()};Ancestry={originalAncestry}");
                    await client1.SendAsync(msg);
                }

                ancestryRecordedInServer2.Should().NotBeEmpty("requests were written to it")
                    .And.BeEquivalentTo(expectedAncestry, "these are the ancestryResults that should have appeared");
            }
        }

        [Fact]
        public async Task TracingHttpMessageHandler_ExceptionOnRequest_ThrowsWrappedHttpRequestException()
        {
            WrappedHttpRequestException wrappedException = null;

            var webHost1Builder = new WebHostBuilder()
                .ConfigureServices(a => a.AddTracing())
                .Configure(a =>
                {
                    a.UseTracing();

                    Func<HttpRequestMessage, Task<HttpResponseMessage>> alwaysThrows = r =>
                        Task.FromException<HttpResponseMessage>(new HttpRequestException("error with request", new InvalidOperationException("inner error")));

                    var sut = a.ApplicationServices.GetRequiredService<TracingHttpMessageHandler>();
                    sut.InnerHandler = new FakeHttpMessageHandler(alwaysThrows);

                    var outerMessageHandler = new FakeHttpMessageHandler(async request =>
                    {
                        try
                        {

                            using (var invoker = new HttpMessageInvoker(sut))
                            {
                                return await invoker.SendAsync(request, CancellationToken.None);
                            }
                        }
                        catch (WrappedHttpRequestException ex)
                        {
                            wrappedException = ex;
                            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                        }
                    });


                    var httpClient = new HttpClient(outerMessageHandler);

                    a.Run(ctx => httpClient.GetAsync("http://test.com/"));
                });

            using (var server1 = new TestServer(webHost1Builder))
            using (var client1 = server1.CreateClient())
            {
                await client1.GetAsync("/");
            }

            wrappedException.Should()
                .NotBeNull("the exception was thrown");
        }
    }
}
