using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Voidwell.Microservice.Tracing;

namespace Voidwell.Microservice.Http.AuthenticatedHttpClient
{
    public static class AuthenticatedHttpClientExtensions
    {
        public static void AddDefaultHttpClient(this IServiceCollection services)
        {
            services.AddTracing();

            services.AddHttpClient(Constants.DefaultTraceHttpClient)
                .AddHttpMessageHandler<TracingHttpMessageHandler>();
        }

        public static void AddAuthenticatedHttpClient<TImplementation>(this IServiceCollection services, Action<AuthenticatedHttpClientOptions> options)
            where TImplementation : class
        {
            services.AddDefaultHttpClient();

            services.AddOptions();
            services.Configure<AuthenticatedHttpClientOptions>(typeof(TImplementation).FullName, options);

            services.TryAddSingleton<IHttpTokenManager, HttpTokenManager>();
            services.TryAddTransient<AuthenticatedHttpMessageHandler<TImplementation>>();

            services.AddHttpClient<TImplementation>()
                .AddHttpMessageHandler<TracingHttpMessageHandler>()
                .AddHttpMessageHandler<AuthenticatedHttpMessageHandler<TImplementation>>();
        }

        public static void AddAuthenticatedHttpClient<TClient, TImplementation>(this IServiceCollection services, Action<AuthenticatedHttpClientOptions> options)
            where TClient : class
            where TImplementation : class, TClient
        {
            services.AddDefaultHttpClient();

            services.AddOptions();
            services.Configure<AuthenticatedHttpClientOptions>(typeof(TClient).FullName, options);

            services.TryAddSingleton<IHttpTokenManager, HttpTokenManager>();
            services.TryAddTransient<AuthenticatedHttpMessageHandler<TClient>>();

            services.AddHttpClient<TClient, TImplementation>()
                .AddHttpMessageHandler<TracingHttpMessageHandler>()
                .AddHttpMessageHandler<AuthenticatedHttpMessageHandler<TClient>>();
        }
    }
}
