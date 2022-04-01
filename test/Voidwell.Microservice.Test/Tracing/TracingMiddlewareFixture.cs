using Voidwell.Microservice.Tracing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Voidwell.Microservice.Test.Tracing
{
    public class TracingMiddlewareFixture
    {
        public RequestDelegate Next { get; set; }
        public TracingOptions Options { get; set; }
        public TraceContext TraceContext { get; set; }
        public ILogger<TracingMiddleware> Logger { get; set; }

        public TracingMiddleware CreateSut()
        {
            return new TracingMiddleware(Next, Options, () => TraceContext, Logger);
        }

        public void ResetFixture()
        {
            Next = Mock.Of<RequestDelegate>();
            Options = new TracingOptions();
            TraceContext = Mock.Of<TraceContext>();
            Logger = Mock.Of<ILogger<TracingMiddleware>>();
        }
    }
}
