using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Voidwell.Microservice.Http
{
    public class JsonContent : StringContent
    {
        public JsonContent(string content) : base(content, Encoding.UTF8, "application/json")
        {
        }

        public static JsonContent FromObject(object objectToSerialize, JsonSerializerOptions options = null)
        {
            var serializedValue = JsonSerializer.Serialize(objectToSerialize, options);
            return new JsonContent(serializedValue);
        }
    }
}
