using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Voidwell.Microservice.Tracing
{
    public class TracingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TracingOptions _options;
        private readonly Func<TraceContext> _traceContextFactory;
        private readonly ILogger<TracingMiddleware> _logger;

        public TracingMiddleware(RequestDelegate next, TracingOptions options, Func<TraceContext> traceContextFactory,
            ILogger<TracingMiddleware> logger)
        {
            _next = next;
            _traceContextFactory = traceContextFactory;
            _logger = logger;
            _options = options;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var traceContext = _traceContextFactory();

            var VoidwellTraceId = httpContext.Request.Headers[TraceContext.VoidwellTraceId].FirstOrDefault();
            traceContext.SetTraceData(VoidwellTraceId);

            if (!httpContext.Items.ContainsKey(TraceContext.TraceContextItemKey))
            {
                httpContext.Items.Add(TraceContext.TraceContextItemKey, traceContext);
            }
            else
            {
                throw new InvalidOperationException($"{nameof(TraceContext)} already exists in HttpContext Items. Check middleware configuration.");
            }

            await _next(httpContext);

            if (!(_options.IgnoreTrace?.Any(a => a(httpContext)) ?? false) && !HasIgnoreEnabledByTraceHeader(traceContext))
            {
                _logger.Log(LogLevel.Information, 10, new RequestCompleteLog(httpContext, traceContext.GetElapsedTime()),
                null, RequestCompleteLog.Callback);
            }
        }

        private bool HasIgnoreEnabledByTraceHeader(TraceContext traceContext)
        {
            return !string.IsNullOrEmpty(traceContext.Data?.NoTraceLog) &&
                   Boolean.TryParse(traceContext.Data?.NoTraceLog, out bool noTrace);
        }
    }
}
