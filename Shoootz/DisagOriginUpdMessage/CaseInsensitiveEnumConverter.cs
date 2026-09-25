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

        if (value is not null && Enum.TryParse(value, true, out TEnum result))
        {
            return result;
        }

        throw new JsonException($"'{value}' is not a valid value for {typeof(TEnum).Name}.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
