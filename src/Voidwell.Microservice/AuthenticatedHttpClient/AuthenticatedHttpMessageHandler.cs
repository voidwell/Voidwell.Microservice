using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Voidwell.Microservice.AuthenticatedHttpClient
{
    public class AuthenticatedHttpMessageHandler<TClient> : DelegatingHandler where TClient : class
    {
        private readonly IHttpTokenManager _httpTokenManager;

        private readonly string _sourceClient;

        public AuthenticatedHttpMessageHandler(IHttpTokenManager httpTokenManager)
        {
            _sourceClient = typeof(TClient).FullName;
            _httpTokenManager = httpTokenManager;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken callerCancellationToken)
        {
            return SetAuthAndSendAsync(request, callerCancellationToken);
        }

        private async Task<HttpResponseMessage> SetAuthAndSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _httpTokenManager.GetTokenAsync(_sourceClient);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
