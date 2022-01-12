using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Voidwell.Microservice.TestApp.Services
{
    public interface ITestAuthenticatedClient
    {
        Task<JsonNode> TestAsync(string characterName);
    }
}