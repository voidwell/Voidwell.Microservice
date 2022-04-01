using System;
using System.Collections.Generic;
using System.Linq;

namespace Voidwell.Microservice.Tracing
{
    public class TraceData
    {
        private readonly Dictionary<string, string> _trace;

        internal const string RootKey = "Root";
        internal const string SelfKey = "Self";
        internal const string AncestryKey = "Ancestry";

        private string _root;
        private string _self;
        private string _ancestry;

        private const string CustomValuePrefix = "Origin";

        public TraceData(string traceStr)
        {
            _trace = (traceStr ?? string.Empty)
                .Split(';')
                .Select(a => a.Split('='))
                .Where(a => a.Count() == 2)
                .ToDictionary(k => k[0], v => v[1]);
        }

        public string Root
        {
            get
            {
                if (_root == null && !_trace.TryGetValue(RootKey, out _root))
                {
                    _root = $"{CustomValuePrefix}-{Guid.NewGuid()}";
                }

                return _root;
            }
        }

        public string Self
        {
            get
            {
                if (_self == null)
                {
                    _trace.TryGetValue(SelfKey, out _self);
                }

                return _self;
            }
        }

        public string Ancestry
        {
            get
            {
                if (_ancestry == null && !_trace.TryGetValue(AncestryKey, out _ancestry))
                {
                    _ancestry = TraceContext.DefaultAncestry;
                }
                return _ancestry;
            }
        }
    }
}
