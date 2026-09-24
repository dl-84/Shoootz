using System.Text.Json.Serialization;

namespace Shoootz.DisagOriginUpdMessage;

[JsonConverter(typeof(CaseInsensitiveEnumConverter<DisagEvaluation>))]
public enum DisagEvaluation
{
    Default = 0,
    ISSF = 1,
    Disposed = 2,
    RedDot = 3,
    ISSF_Qualification = 4,
}
