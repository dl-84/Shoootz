using System.ComponentModel.DataAnnotations;

namespace Shoootz.Store.Entities;

public class ClubEntity
{
    public int Id { get; set; }

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public ICollection<ShooterEntity> Shooters { get; set; } = [];
}
