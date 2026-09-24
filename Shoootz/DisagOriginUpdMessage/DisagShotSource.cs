using System.Text.Json.Serialization;

namespace Shoootz.DisagOriginUpdMessage;

[JsonConverter(typeof(CaseInsensitiveEnumConverter<DisagShotSource>))]
public enum DisagShotSource
{
    OpticScore = 0,
    RedDot = 1,
}
