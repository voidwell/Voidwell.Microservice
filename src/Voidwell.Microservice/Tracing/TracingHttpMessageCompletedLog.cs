using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;

namespace Voidwell.Microservice.Tracing
{
    internal class TracingHttpMessageCompletedLog : IReadOnlyList<KeyValuePair<string, object>>
    {
        internal static readonly Func<object, Exception, string> Callback =
            (state, exception) => ((TracingHttpMessageCompletedLog)state).ToString();

        private readonly HttpRequestMessage _httpRequest;
        private readonly HttpResponseMessage _httpResponse;
        private readonly TimeSpan _serviceElapsedTime;
        private readonly TimeSpan _requestElapsedTime;

        private string _cachedToString;

        public TracingHttpMessageCompletedLog(HttpRequestMessage httpRequest, HttpResponseMessage httpResponse, 
            TimeSpan requestElapsedTime, TimeSpan serviceElapsedTime)
        {
            _httpRequest = httpRequest;
            _httpResponse = httpResponse;
            _requestElapsedTime = requestElapsedTime;
            _serviceElapsedTime = serviceElapsedTime;
        }

        public int Count => 5;

        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return new KeyValuePair<string, object>("RequestMethod", _httpRequest.Method);
                    case 1:
                        return new KeyValuePair<string, object>("RequestUri", _httpRequest.RequestUri);
                    case 2:
                        return new KeyValuePair<string, object>("ResponseStatusCode", _httpResponse.StatusCode);
                    case 3:
                        return new KeyValuePair<string, object>("RequestElapsedMilliseconds", _requestElapsedTime.TotalMilliseconds);
                    case 4:
                        return new KeyValuePair<string, object>("ServiceElapsedMilliseconds", _serviceElapsedTime.TotalMilliseconds);
                    default:
                        throw new IndexOutOfRangeException(nameof(index));
                }
            }
        }

        public override string ToString()
        {
            if (_cachedToString == null)
            {
                _cachedToString = string.Format(
                    CultureInfo.InvariantCulture,
                    "Completed external request to {0} {1} in {2}ms ({3})",
                    _httpRequest.Method.Method.ToUpper(),
                    _httpRequest.RequestUri,
                    _requestElapsedTime.TotalMilliseconds,
                    _httpResponse.StatusCode);
            }

            return _cachedToString;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
