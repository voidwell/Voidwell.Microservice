using Newtonsoft.Json;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Graylog.Core;
using System;
using System.IO;

namespace Voidwell.Microservice.Logging.GelfConsoleSink
{
    public class GraylogConsoleSink : ILogEventSink
    {
        private readonly Lazy<IGelfConverter> _converter;
        private readonly JsonSerializer _serializer;

        public GraylogConsoleSink()
            : this(new GraylogConsoleSinkOptions())
        {
        }

        public GraylogConsoleSink(GraylogConsoleSinkOptions options)
        {
            var graylogOptions = options.ConvertOptions();
            _serializer = JsonSerializer.Create(graylogOptions.SerializerSettings);
            ISinkComponentsBuilder sinkComponentsBuilder = new SinkComponentsBuilder(graylogOptions);
            _converter = new Lazy<IGelfConverter>(() => sinkComponentsBuilder.MakeGelfConverter());
        }

        public void Emit(LogEvent logEvent)
        {
            try
            {
                var json = _converter.Value.GetGelfJson(logEvent);

                using var textWriter = new StringWriter();
                {
                    _serializer.Serialize(textWriter, json);
                    Console.WriteLine(textWriter.ToString());
                }
            }
            catch (Exception exc)
            {
                SelfLog.WriteLine("Oops something going wrong {0}", exc);
            }
        }
    }
}
