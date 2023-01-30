using FluentAssertions;
using System.Text.Json;
using Voidwell.Microservice.Hosting;
using Xunit;

namespace Voidwell.Microservice.Test.Hosting
{
    public class DateTimeJsonConverterTests
    {
        [Fact]
        public void Converts_Value_To_UTC()
        {
            // Arrange
            var datetime = DateTime.Parse("2022-01-13T16:25:35.1250000-05:00");
            var serializerOptions = new JsonSerializerOptions();
            serializerOptions.Converters.Add(new DateTimeJsonConverter());

            // Act
            var result = JsonSerializer.Serialize(datetime, serializerOptions);

            // Assert
            result.Should()
                .Be("\"2022-01-13T21:25:35.125Z\"");
        }
    }
}
