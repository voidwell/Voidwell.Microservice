using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IdentityServer4.AccessTokenValidation;
using System;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Voidwell.Microservice.Extensions;
using Voidwell.Microservice.Logging.Enrichers;

namespace Voidwell.Microservice.Tracing
{
    public static class TracingExtensions
    {
        public static IServiceCollection AddTracing(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.TryAddTransient<TracingHttpMessageHandler>();

            services.TryAddTransient<Func<TraceContext>>(a => () => new TraceContext());
            services.TryAddSingleton<ITraceContextAccessor, TraceContextAccessor>();
            services.TryAddTransient<TracingEnricher>();

            services.PostConfigure<IdentityServerAuthenticationOptions>((sp, name, options) =>
            {
                var jwtBackChannelHandler = sp.GetRequiredService<TracingHttpMessageHandler>();
                jwtBackChannelHandler.InnerHandler = options.JwtBackChannelHandler;
                options.JwtBackChannelHandler = jwtBackChannelHandler;
            });

            services.AddHttpClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName)
                .AddHttpMessageHandler<TracingHttpMessageHandler>();

            return services;
        }

        public static IApplicationBuilder UseTracing(this IApplicationBuilder app)
        {
            return app.UseTracing(new TracingOptions());
        }

        public static IApplicationBuilder UseTracing(this IApplicationBuilder app, TracingOptions options)
        {
            return app.UseMiddleware<TracingMiddleware>(options);
        }
    }
}
