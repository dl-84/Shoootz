using System;

namespace Shoootz.Models.Shot;

internal class ShotModel
{
    public int Count { get; set; }

    public double DecValue { get; set; }

    public string? DiscType { get; set; }

    public double Distance { get; set; }

    public bool IsHot { get; set; }

    public bool IsValid { get; set; }

    public ShotInfoModel? ShotInfo { get; set; }

    public DateTime ShotDateTime { get; set; }

    public ShooterModel? Shooter { get; set; }
}
