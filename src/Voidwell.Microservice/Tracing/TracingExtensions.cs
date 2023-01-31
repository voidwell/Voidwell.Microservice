using Microsoft.AspNetCore.Builder;
using Voidwell.Microservice.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Voidwell.Microservice.Logging.Enrichers;
using Microsoft.AspNetCore.Authentication.JwtBearer;


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

            services.PostConfigure<JwtBearerOptions>((sp, name, options) =>
            {
                if (options == null)
                {
                    return;
                }

                var jwtBackChannelHandler = sp.GetRequiredService<TracingHttpMessageHandler>();
                jwtBackChannelHandler.InnerHandler = options.BackchannelHttpHandler;
                options.BackchannelHttpHandler = jwtBackChannelHandler;
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
