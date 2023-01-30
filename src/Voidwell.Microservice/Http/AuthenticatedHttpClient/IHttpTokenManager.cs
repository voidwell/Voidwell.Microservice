using System.Threading.Tasks;

namespace Voidwell.Microservice.Http.AuthenticatedHttpClient
{
    public interface IHttpTokenManager
    {
        Task<string> GetTokenAsync(string clientName);
    }
}