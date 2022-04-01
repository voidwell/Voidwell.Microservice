namespace Voidwell.Microservice.Test
{
    public class FakeWrappedHttpMessageHandler : DelegatingHandler
    {
        private readonly HttpMessageInvoker _invoker;
        private readonly Func<HttpRequestMessage, Task> _action;

        public FakeWrappedHttpMessageHandler(Func<HttpRequestMessage, Task> action,
            HttpMessageHandler innerMessageHandler)
        {
            _invoker = new HttpMessageInvoker(innerMessageHandler, false);
            _action = action;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            await _action(request);
            return await _invoker.SendAsync(request, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            _invoker.Dispose();
            base.Dispose(disposing);
        }
    }
}
