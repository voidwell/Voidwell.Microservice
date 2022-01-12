using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System;

namespace Voidwell.Microservice.Logging
{
    public static class LoggingExtensions
    {
        public static IWebHostBuilder UseMicroserviceLogging(this IWebHostBuilder builder,
            params ILogEventEnricher[] enrichers)
        {
            return UseMicroserviceLogging(builder, new LoggingOptions(), enrichers);
        }

        public static IWebHostBuilder UseMicroserviceLogging(this IWebHostBuilder builder,
            LoggingOptions loggingOptions, params ILogEventEnricher[] enrichers)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ConfigureServices(services =>
                services.AddSingleton<ILoggerFactory>(provider =>
                    new ConfiguredLoggerFactory(provider, loggingOptions, enrichers)));

            return builder;
        }

    }
}
