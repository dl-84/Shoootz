using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shoootz.Context.Entities;

internal class ShooterEntity
{
    public int Birthyear { get; set; }

    [MaxLength(255)]
    public string? Club { get; set; }

    [MaxLength(100)]
    public string? Firstname { get; set; }

    [MaxLength(10)]
    public string Id { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Lastname { get; set; }

    public ICollection<ShotEntity> Shots { get; set; } = [];

    public int Startnumber { get; set; }

    [MaxLength(255)]
    public string? Team { get; set; }
}
