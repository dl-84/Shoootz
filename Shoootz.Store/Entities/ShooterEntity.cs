using System.ComponentModel.DataAnnotations;
using Shoootz.Store.Types;

namespace Shoootz.Store.Entities;

public class ShooterEntity
{
    public int Birthyear { get; set; }

    public ClubEntity? Club { get; set; }

    public int? ClubId { get; set; }

    [MaxLength(100)]
    public string? Firstname { get; set; }

    public GenderType? Gender { get; set; }

    [MaxLength(10)]
    public string Id { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Lastname { get; set; }

    public ICollection<ShotEntity> Shots { get; set; } = [];

    public int Startnumber { get; set; }

    [MaxLength(255)]
    public string? Team { get; set; }
}
