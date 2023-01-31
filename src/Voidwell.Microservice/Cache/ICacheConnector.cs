using StackExchange.Redis;
using System.Threading;
using System.Threading.Tasks;

namespace Voidwell.Microservice.Cache
{
    public interface ICacheConnector
    {
        IDatabase Database { get; }
        Task<IDatabaseAsync> ConnectAsync(CancellationToken cancellationToken = default);
    }
}
