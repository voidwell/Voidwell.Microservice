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
            return UseMicroserviceLogging(builder, null, enrichers);
        }

        public static IWebHostBuilder UseMicroserviceLogging(this IWebHostBuilder builder,
            Action<LoggingOptions> loggingOptions, params ILogEventEnricher[] enrichers)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var options = new LoggingOptions();
            if (loggingOptions != null)
            {
                loggingOptions.Invoke(options);
            }

            builder.ConfigureServices(services =>
                services.AddSingleton<ILoggerFactory>(provider =>
                    new ConfiguredLoggerFactory(provider, options, enrichers)));

            return builder;
        }

    }
}
