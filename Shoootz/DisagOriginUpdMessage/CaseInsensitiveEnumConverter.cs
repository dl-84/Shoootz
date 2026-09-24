using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shoootz.DisagOriginUpdMessage;

public class CaseInsensitiveEnumConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();

        return value is not null && Enum.TryParse(value, true, out TEnum result) ? result : default;
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
