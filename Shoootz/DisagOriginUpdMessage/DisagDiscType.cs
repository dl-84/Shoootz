using System.Text.Json.Serialization;

namespace Shoootz.DisagOriginUpdMessage;

[JsonConverter(typeof(CaseInsensitiveEnumConverter<DisagDiscType>))]
public enum DisagDiscType
{
    Default = 0,
    LG = 1,
    LP = 2,
    LGA = 3,
    LPA = 4,
    KK = 5,
    KKA = 6,
    ZS = 7,
    ZSA = 8,
    KYFFH = 9,
    ZSTRD = 10,
}
