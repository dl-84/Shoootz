using System.ComponentModel.DataAnnotations;

namespace Shoootz.Store.Entities;

public class ShotChecksumEntity
{
    [MaxLength(64)]
    public string Hash { get; set; } = string.Empty;

    public int ShotId { get; set; }
}
