using System.Text.Json.Serialization;

namespace Shoootz.Models.Udp;

internal class UdpShotDetailModel
{
    public string? Competition { get; set; }

    public int Count { get; set; }

    public double DecValue { get; set; }

    public string? DiscType { get; set; }

    public string? DiscTypeRaw { get; set; }

    public double Distance { get; set; }

    public int FullValue { get; set; }

    public bool IsDummy { get; set; }

    public bool IsHot { get; set; }

    public bool IsInnerten { get; set; }

    public bool IsShootoff { get; set; }

    public bool IsValid { get; set; }

    public bool IsWarmup { get; set; }

    public int LastTLChange { get; set; }

    public UdpMenuItemModel? MenuItem { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UdpObjectType? ObjectType { get; set; }

    public int Range { get; set; }

    public string? Remark { get; set; }

    public int Run { get; set; }

    public UdpShooterModel? Shooter { get; set; }

    public string? ShotDateTime { get; set; }

    public string? Source { get; set; }

    public string? TLStatus { get; set; }

    public int X { get; set; }

    public int Y { get; set; }
}
