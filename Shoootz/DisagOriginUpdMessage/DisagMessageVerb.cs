using System.Text.Json.Serialization;

namespace Shoootz.DisagOriginUpdMessage;

[JsonConverter(typeof(CaseInsensitiveEnumConverter<DisagMessageVerb>))]
public enum DisagMessageVerb
{
    Various = 0,
    Shot = 1,
    Series = 2,
    Result = 3,
    TLightSequence = 4,
    LeaguecompetitionStats = 5,
}
