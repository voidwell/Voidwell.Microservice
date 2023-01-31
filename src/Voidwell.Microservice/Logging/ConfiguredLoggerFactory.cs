using Voidwell.Microservice.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Filters;
using System;
using Voidwell.Microservice.Logging.Enrichers;
using Voidwell.Microservice.Logging.GelfConsoleSink;

namespace Voidwell.Microservice.Logging
{
    public class ConfiguredLoggerFactory : ILoggerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SerilogLoggerProvider _provider;

        public ConfiguredLoggerFactory(IServiceProvider serviceProvider, LoggingOptions loggingOptions,
             ILogEventEnricher[] enrichers)
        {
            _serviceProvider = serviceProvider;
            var config = CreateLoggerConfiguration(loggingOptions, enrichers);

            _provider = new SerilogLoggerProvider(config.CreateLogger(), false);
        }

        public void AddProvider(ILoggerProvider provider)
        {
            SelfLog.WriteLine("Ignoring added logger provider {0}", provider);
        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return _provider.CreateLogger(categoryName);
        }

        public void Dispose()
        {
            _provider.Dispose();
        }

        private LoggerConfiguration CreateLoggerConfiguration(LoggingOptions loggingOptions,
             ILogEventEnricher[] enrichers)
        {
            var loggerConfig = new LoggerConfiguration()
                  .MinimumLevel.Is(loggingOptions.MinLogLevel)
                  .Enrich.FromLogContext()
                  .Enrich.WithProperty("ContainerId", Environment.MachineName)
                  .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("Environment"))
                  .Enrich.With(enrichers);

            if (!loggingOptions.IncludeMicrosoftInformation)
            {
                loggerConfig = loggerConfig.Filter.ByExcluding(ExcludeMicrosoftInformation);
            }

            loggingOptions?.IgnoreRules?.ForEach(rule => loggerConfig = loggerConfig.Filter.ByExcluding(rule));

            var serviceProperties = _serviceProvider.GetService<ServiceProperties>();

            if (serviceProperties == null)
            {
                SelfLog.WriteLine("Service Properties not configured in DI. Cannot get application name");
            }
            else
            {
                loggerConfig = loggerConfig.Enrich.WithProperty("Service", serviceProperties.Name);
            }

            var tracingEnricher = _serviceProvider.GetService<TracingEnricher>();

            if (tracingEnricher == null)
            {
                SelfLog.WriteLine("Tracing Enricher was not configured in DI. Cannot adding tracing to logging");
            }
            else
            {
                loggerConfig = loggerConfig.Enrich.With(tracingEnricher);
            }

            var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();

            if (httpContextAccessor == null)
            {
                SelfLog.WriteLine("HttpContextAccessor not configured in DI. Cannot adding ClaimsPrincipal logging");
            }
            else
            {
                loggerConfig = loggerConfig.Enrich.With(new ClaimsPrincipalEnricher(httpContextAccessor));
            }

            if (string.Equals(loggingOptions.LoggingOutput ?? Environment.GetEnvironmentVariable("LoggingOutput"), "flat", StringComparison.OrdinalIgnoreCase))
            {
                loggerConfig.WriteTo.Console(outputTemplate: "[{Level:u4} {Timestamp:HH:mm:ss.fff}] {SourceContext} {Message} {Exception} Trace: {TraceRoot} {TraceAncestry}{NewLine}");
            }
            else
            {
                loggerConfig.WriteTo.Sink<GraylogConsoleSink>();
            }

            return loggerConfig;
        }

        private static bool ExcludeMicrosoftInformation(LogEvent evt)
        {
            return !(IsFilterSourceIncluded(evt, "Microsoft.AspNetCore.Mvc", LogEventLevel.Error)
                && IsFilterSourceIncluded(evt, "Microsoft.AspNetCore", LogEventLevel.Warning)
                && IsFilterSourceIncluded(evt, "System.Net.Http.HttpClient", LogEventLevel.Warning));
        }

        private static bool IsFilterSourceIncluded(LogEvent evt, string source, LogEventLevel minLogLevel)
        {
            return evt.Level < minLogLevel && !Matching.FromSource(source)(evt);
        }
    }
}
