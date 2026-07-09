using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RX.Nyss.Web.Utils
{
    public class JsonStringDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return default;
                }
                
                // Try parsing as ISO 8601 format (with or without timezone)
                if (DateTimeOffset.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
                {
                    return result;
                }
                
                // Fallback: try parsing as DateTime and convert to DateTimeOffset
                if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTime))
                {
                    return new DateTimeOffset(dateTime, TimeSpan.Zero);
                }
            }
            
            return default;
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", CultureInfo.InvariantCulture));
        }
    }
}