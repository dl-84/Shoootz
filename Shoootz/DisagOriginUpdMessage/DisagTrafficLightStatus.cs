using System.Text.Json.Serialization;

namespace Shoootz.DisagOriginUpdMessage;

[JsonConverter(typeof(CaseInsensitiveEnumConverter<DisagTrafficLightStatus>))]
public enum DisagTrafficLightStatus
{
    Red = 0,
    Green = 1,
    Off = 2,
}
