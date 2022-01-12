using Microsoft.Extensions.DependencyInjection;
using System;

namespace Voidwell.Microservice.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection ConfigureServiceProperties(this IServiceCollection services, string serviceName)
        {
            return ConfigureServiceProperties(services, c => c.Name = serviceName);
        }

        public static IServiceCollection ConfigureServiceProperties(this IServiceCollection services, Action<ServiceProperties> serviceProperties)
        {
            services.AddOptions();
            services.Configure<ServiceProperties>(serviceProperties);

            return services;
        }
    }
}
