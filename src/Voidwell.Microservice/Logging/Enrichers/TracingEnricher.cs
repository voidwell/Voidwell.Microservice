using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;
using System.Collections.Generic;
using System.Linq;
using Voidwell.Microservice.Tracing;

namespace Voidwell.Microservice.Logging.Enrichers
{
    public class TracingEnricher : ILogEventEnricher
    {
        private readonly ITraceContextAccessor _traceContextAccessor;

        public TracingEnricher(ITraceContextAccessor traceContextAccessor)
        {
            _traceContextAccessor = traceContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (Matching.FromSource<TraceContextAccessor>()(logEvent))
            {
                // This avoids a stack overflow that occurs in unit testing when a request is not set
                return;
            }

            var traceContext = _traceContextAccessor.TraceContext;

            if (traceContext?.Data == null)
            {
                return;
            }

            GetProperties(propertyFactory).ToList()
                .ForEach(logEvent.AddPropertyIfAbsent);
        }

        public IEnumerable<LogEventProperty> GetProperties(ILogEventPropertyFactory propertyFactory)
        {
            var traceContext = _traceContextAccessor.TraceContext;

            yield return propertyFactory.CreateProperty("TraceRoot", traceContext.Data.Root);

            if (traceContext.Data.Self != null)
            {
                yield return propertyFactory.CreateProperty("TraceSelf", traceContext.Data.Self);
            }

            yield return propertyFactory.CreateProperty("TraceAncestry", traceContext.Data.Ancestry);
        }
    }
}
