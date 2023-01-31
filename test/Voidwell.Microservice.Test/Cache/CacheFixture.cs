using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;
using StackExchange.Redis;
using System.Text.Json;
using Voidwell.Microservice.Cache;

namespace Voidwell.Microservice.Test.Cache
{
    public class CacheFixture
    {
        public ICacheConnector CacheConnector { get; set; }
        public CacheOptions CacheOptions { get; set; }

        public IDatabase RedisDatabase { get; set; }

        public Microservice.Cache.Cache CreateSut()
        {
            var options = Options.Create(CacheOptions);
            return new Microservice.Cache.Cache(CacheConnector, options);
        }

        public IReturnsResult<IDatabase> MockCacheGet(string key, object value)
        {
            return RedisDatabase.AsMock()
                .Setup(a => a.StringGetAsync(key, It.IsAny<CommandFlags>()))
                .ReturnsAsync(JsonSerializer.Serialize(value));
        }

        public void ResetFixture()
        {
            CacheOptions = new CacheOptions();
            RedisDatabase = Mock.Of<IDatabase>();

            CacheConnector = Mock.Of<ICacheConnector>();
            CacheConnector.AsMock()
                .Setup(a => a.ConnectAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(RedisDatabase);
            
        }
    }
}
