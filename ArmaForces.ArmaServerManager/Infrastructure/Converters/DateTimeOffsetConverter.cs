using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArmaForces.ArmaServerManager.Infrastructure.Converters
{
    internal class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        private const string DateTimeFormat = "yyyy-MM-dd'T'HH:mm:sszzz";
        
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert == typeof(DateTimeOffset);

        public override DateTimeOffset Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
            => DateTimeOffset.ParseExact(reader.GetString() ?? string.Empty,
                DateTimeFormat, CultureInfo.InvariantCulture);

        public override void Write(
            Utf8JsonWriter writer,
            DateTimeOffset dateTimeValue,
            JsonSerializerOptions options)
            => writer.WriteStringValue(dateTimeValue.ToString(
                DateTimeFormat, CultureInfo.InvariantCulture));
    }
}
