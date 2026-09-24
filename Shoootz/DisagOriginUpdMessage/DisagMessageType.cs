using System.Text.Json.Serialization;

namespace Shoootz.DisagOriginUpdMessage;

[JsonConverter(typeof(CaseInsensitiveEnumConverter<DisagMessageType>))]
public enum DisagMessageType
{
    Command = 0,
    Event = 1,
}
