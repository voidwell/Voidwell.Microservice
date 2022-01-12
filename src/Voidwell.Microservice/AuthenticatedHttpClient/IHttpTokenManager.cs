using System.Threading.Tasks;

namespace Voidwell.Microservice.AuthenticatedHttpClient
{
    public interface IHttpTokenManager
    {
        Task<string> GetTokenAsync(string clientName);
    }
}