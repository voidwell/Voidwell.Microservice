using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace Voidwell.Microservice.Extensions
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsObjectAsync<T>(this HttpContent content)
        {
            var serializedString = await content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(serializedString);
        }
    }
}
