using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shoootz.DisagOriginUpdMessage;

public class DisagDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();

        return value is not null ? DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.None) : default;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
    }
}
