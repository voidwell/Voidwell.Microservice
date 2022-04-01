using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Voidwell.Microservice.Test
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _action;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> action)
            : this(req => Task.FromResult(action(req)))
        {
        }

        public FakeHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> action)
            : this((req, token) => action(req))
        {
        }

        public FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _action = action;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _action(request, cancellationToken);
        }
    }
}
