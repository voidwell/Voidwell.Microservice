using Voidwell.Microservice.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voidwell.Microservice.Test.Tracing
{
    public class SingletonServiceWithTracing
    {
        private readonly ITraceContextAccessor _traceContextAccesssor;

        public SingletonServiceWithTracing(ITraceContextAccessor traceContextAccessor)
        {
            _traceContextAccesssor = traceContextAccessor;
        }

        public string GetRoot()
        {
            var traceContext = _traceContextAccesssor.TraceContext;

            if (traceContext.Data == null)
            {
                throw new InvalidOperationException("Data should not be null");
            }

            return traceContext.Data.Root;
        }
    }
}
