using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using Voidwell.Microservice.Configuration;
using Voidwell.Microservice.Extensions;

namespace Voidwell.Microservice.Cache
{
    public static class CacheExtensions
    {
        public static IServiceCollection AddCache(this IServiceCollection services, Action<CacheOptions> actionOptions)
        {
            services.AddOptions();
            services.Configure<CacheOptions>(actionOptions);

            services.PostConfigure<CacheOptions>((sp, name, options) =>
            {
                if (string.IsNullOrWhiteSpace(options.KeyPrefix))
                {
                    options.KeyPrefix = sp.GetService<IOptions<ServiceProperties>>()?.Value?.Name;
                }
            });

            services.TryAddSingleton<ICacheConnector, CacheConnector>();
            services.TryAddSingleton<ICache, Cache>();

            return services;
        }
    }
}
