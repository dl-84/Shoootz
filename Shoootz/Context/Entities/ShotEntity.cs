using System;
using System.ComponentModel.DataAnnotations;

namespace Shoootz.Context.Entities;

internal class ShotEntity
{
    public int Count { get; set; }

    public double DecValue { get; set; }

    [MaxLength(10)]
    public string? DiscType { get; set; }

    public double Distance { get; set; }

    public int Id { get; set; }

    public bool IsHot { get; set; }

    public bool IsValid { get; set; }

    public DateTime ShotDateTime { get; set; }

    [MaxLength(10)]
    public string? ShooterId { get; set; }

    public ShooterEntity? Shooter { get; set; }

    [MaxLength(50)]
    public string? ShotInfoMenuId { get; set; }

    public ShotInfoEntity? ShotInfo { get; set; }
}
