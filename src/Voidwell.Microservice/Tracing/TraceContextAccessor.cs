using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;

namespace Voidwell.Microservice.Tracing
{
    public class TraceContextAccessor : ITraceContextAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TraceContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public TraceContext TraceContext
        {
            get
            {
                object traceContextObj;

                if (_httpContextAccessor.HttpContext == null)
                {
                    return null;
                }

                if (!_httpContextAccessor.HttpContext.Items.TryGetValue(TraceContext.TraceContextItemKey, out traceContextObj))
                {
                    // Outside the bounds of a request
                    return null;
                }

                var traceContext = traceContextObj as TraceContext;

                if (traceContext == null)
                {
                    throw new InvalidOperationException($"The value in HttpContext Items was not the expected type of {nameof(TraceContext)}.");
                }

                return traceContext;
            }
        }
    }
}
