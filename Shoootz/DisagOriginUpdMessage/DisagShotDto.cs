using System;
using System.Text.Json.Serialization;

namespace Shoootz.DisagOriginUpdMessage;

public class DisagShotDto : DisagStructureBaseDto
{
    [JsonConverter(typeof(DisagDateTimeConverter))]
    public DateTime ShotDateTime { get; set; }

    public DisagTrafficLightStatus TLStatus { get; set; }

    public int LastTLChange { get; set; }

    public DisagShotSource Source { get; set; }

    public int Range { get; set; }

    public DisagShooterDto? Shooter { get; set; }

    public DisagCompetitionDto? Competition { get; set; }

    public DisagDiscType DiscType { get; set; }

    public string? DiscTypeRaw { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public decimal Distance { get; set; }

    public int Count { get; set; }

    public int FullValue { get; set; }

    public decimal DecValue { get; set; }

    public int Run { get; set; }

    public bool IsValid { get; set; }

    public bool IsWarmup { get; set; }

    public bool IsHot { get; set; }

    public bool IsDummy { get; set; }

    public bool IsInnerten { get; set; }

    public bool IsShootoff { get; set; }

    public DisagMenuItemDto? MenuItem { get; set; }

    public string? Remark { get; set; }
}
