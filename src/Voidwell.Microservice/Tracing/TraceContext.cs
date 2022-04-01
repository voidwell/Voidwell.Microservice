using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Voidwell.Microservice.Tracing
{
    public class TraceContext
    {
        public const string VoidwellTraceId = "X-Void-Trace-Id";
        public const string TraceContextItemKey = "Trace-Context";
        public const string DefaultAncestry = "X";

        private int _requestCount;


        public TraceContext()
        {
            _requestCount = -1;
            StartTimestamp = Stopwatch.GetTimestamp();
        }

        public TraceData Data { get; private set; }
        public long StartTimestamp { get; }

        public virtual void SetTraceData(string traceStr)
        {
            if (Data != null)
            {
                throw new InvalidOperationException("Attempted to set data a second time");
            }

            Data = new TraceData(traceStr);
        }

        private int GetRequestCount()
        {
            return Interlocked.Increment(ref _requestCount);
        }

        public string GetNextRequestHeader()
        {
            var items = GetTraceHeaderForNextRequest()
                .Select(a => $"{a.Key}={a.Value}");

            return string.Join(";", items);
        }

        private IEnumerable<KeyValuePair<string, string>> GetTraceHeaderForNextRequest()
        {
            yield return new KeyValuePair<string, string>(TraceData.RootKey, Data.Root);

            var self = Data.Self;

            if (self != null)
            {
                yield return new KeyValuePair<string, string>(TraceData.SelfKey, self);
            }

            yield return new KeyValuePair<string, string>(TraceData.AncestryKey,
                $"{Data.Ancestry}:{GetRequestCount()}");
        }

        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        public TimeSpan GetElapsedTime()
        {
            return new TimeSpan((long)(TimestampToTicks * (Stopwatch.GetTimestamp() - StartTimestamp)));
        }
    }
}
