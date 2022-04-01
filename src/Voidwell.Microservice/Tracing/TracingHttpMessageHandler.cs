using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Voidwell.Microservice.Tracing
{
    public class TracingHttpMessageHandler : DelegatingHandler
    {
        private readonly ITraceContextAccessor _traceContextAccessor;
        private readonly ILogger<TracingHttpMessageHandler> _logger;

        public TracingHttpMessageHandler(ITraceContextAccessor traceContextAccessor, ILogger<TracingHttpMessageHandler> logger)
        {
            if (traceContextAccessor == null)
            {
                throw new ArgumentNullException(nameof(traceContextAccessor));
            }

            _traceContextAccessor = traceContextAccessor;
            _logger = logger;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var timer = Stopwatch.StartNew();

            var context = _traceContextAccessor.TraceContext;
            bool traceContextExists = false;

            if (context == null)
            {
                _logger.LogTrace($"{nameof(TraceContext)} was not found from HttpContext");
            }
            else if (context.Data == null)
            {
                _logger.LogWarning($"{nameof(TraceData)} was not applied to {nameof(TraceContext)}. Check to make sure middleware was added.");
            }
            else
            {
                request.Headers.Remove(TraceContext.VoidwellTraceId);
                request.Headers.Add(TraceContext.VoidwellTraceId, context.GetNextRequestHeader());
                traceContextExists = true;
            }

            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                if (traceContextExists)
                {
                    var tracingHttpMessageCompletedLog = new TracingHttpMessageCompletedLog(request, response,
                        timer.Elapsed, context?.GetElapsedTime() ?? TimeSpan.FromTicks(0));
                    _logger.Log(LogLevel.Information, 11,
                        tracingHttpMessageCompletedLog, null, TracingHttpMessageCompletedLog.Callback);
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                throw new WrappedHttpRequestException(ex, request);
            }
        }
    }
}
