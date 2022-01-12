using Serilog.Events;
using Serilog.Sinks.Graylog.Core.Helpers;
using Serilog.Sinks.Graylog.Core;
using Serilog.Sinks.Graylog;
using Newtonsoft.Json;

namespace Voidwell.Microservice.Logging.GelfConsoleSink
{
    public class GraylogConsoleSinkOptions
    {
        public GraylogConsoleSinkOptions()
        {
            var baseOptions = new GraylogSinkOptions();
            MessageGeneratorType = baseOptions.MessageGeneratorType;
            ShortMessageMaxLength = baseOptions.ShortMessageMaxLength;
            MinimumLogEventLevel = baseOptions.MinimumLogEventLevel;
            StackTraceDepth = baseOptions.StackTraceDepth;
            IncludeMessageTemplate = baseOptions.IncludeMessageTemplate;
            MessageTemplateFieldName = baseOptions.MessageTemplateFieldName;
            SerializerSettings = baseOptions.SerializerSettings;
        }

        public JsonSerializerSettings SerializerSettings { get; set; }
        public int StackTraceDepth { get; set; }
        public MessageIdGeneratorType MessageGeneratorType { get; set; }
        public int ShortMessageMaxLength { get; set; }
        public LogEventLevel MinimumLogEventLevel { get; set; }
        public bool IncludeMessageTemplate { get; set; }
        public string MessageTemplateFieldName { get; set; }
        public IGelfConverter GelfConverter { get; set; }

        public GraylogSinkOptions ConvertOptions()
        {
            return new GraylogSinkOptions
            {
                SerializerSettings = SerializerSettings,
                StackTraceDepth = StackTraceDepth,
                MessageGeneratorType = MessageGeneratorType,
                ShortMessageMaxLength = ShortMessageMaxLength,
                MinimumLogEventLevel = MinimumLogEventLevel,
                IncludeMessageTemplate = IncludeMessageTemplate,
                MessageTemplateFieldName = MessageTemplateFieldName,
                GelfConverter = GelfConverter
            };
        }
    }
}
