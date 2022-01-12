using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace Voidwell.Microservice.Extensions
{
    public static class PostConfigureExtensions
    {
        public static IServiceCollection PostConfigure<TOptions>(this IServiceCollection services, Action<IServiceProvider, string, TOptions> configureAction) where TOptions : class
        {
            services.TryAddSingleton<IPostConfigureOptions<TOptions>>(sp => new PostConfigurationWrapper<TOptions>(sp, configureAction));

            return services;
        }
    }
}
