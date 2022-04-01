using Voidwell.Microservice.Tracing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Voidwell.Microservice.Test.Tracing
{
    public class TracingMiddlewareTest : IClassFixture<TracingMiddlewareFixture>
    {
        private readonly TracingMiddlewareFixture _fixture;

        public TracingMiddlewareTest(TracingMiddlewareFixture fixture)
        {
            _fixture = fixture;
            _fixture.ResetFixture();
        }

        [Fact]
        public async Task Invoke_HeaderInRequest_ContextSetAndSaved()
        {
            var header = $"header-{Guid.NewGuid()}";

            var items = Mock.Of<IDictionary<object, object>>();
            var headers = Mock.Of<IHeaderDictionary>(ctx => 
                ctx[TraceContext.VoidwellTraceId] == new StringValues(header));
            var httpContext = Mock.Of<HttpContext>(ctx => 
                ctx.Request.Headers == headers && ctx.Items == items);

            var sut = _fixture.CreateSut();

            await sut.Invoke(httpContext);

            _fixture.TraceContext.AsMock()
                .Verify(a => a.SetTraceData(header), "the header was not set to the context");
            httpContext.Items.AsMock()
                .Verify(a => a.Add(TraceContext.TraceContextItemKey, _fixture.TraceContext), 
                    "trace context was not stored");
            _fixture.Next.AsMock()
                .Verify(a => a(httpContext), "failed to call next");
        }

        [Fact]
        public async Task Invoke_NoHeaderInRequest_ContextSetAndSaved()
        {
            var items = Mock.Of<IDictionary<object, object>>();
            var headers = Mock.Of<IHeaderDictionary>();

            var httpContext = Mock.Of<HttpContext>(ctx =>
                ctx.Request.Headers == headers && ctx.Items == items);

            var sut = _fixture.CreateSut();

            await sut.Invoke(httpContext);

            _fixture.TraceContext.AsMock()
                .Verify(a => a.SetTraceData(null), "the setup was not called");
            httpContext.Items.AsMock()
                .Verify(a => a.Add(TraceContext.TraceContextItemKey, _fixture.TraceContext),
                    "trace context was not stored");
            _fixture.Next.AsMock()
                .Verify(a => a(httpContext), "failed to call next");
        }

        [Fact]
        public async Task Invoke_TraceContextAlreadyStore_ThrowsInvalidOperationException()
        {
            var items = new Dictionary<object, object> { { TraceContext.TraceContextItemKey, new TraceContext() } };
            var headers = Mock.Of<IHeaderDictionary>();
            var httpContext = Mock.Of<HttpContext>(ctx =>
                ctx.Request.Headers == headers && ctx.Items == items);

            var sut = _fixture.CreateSut();

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Invoke(httpContext));

            _fixture.Next.AsMock()
                .Verify(a => a(httpContext), Times.Never, "this should not have been called");
        }
    }
}
