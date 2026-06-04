using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shoootz.Store.Entities;

public class ShotInfoEntity
{
    [MaxLength(50)]
    public string MenuId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? MenuItemName { get; set; }

    [MaxLength(100)]
    public string? MenuPointName { get; set; }

    public ICollection<ShotEntity> Shots { get; set; } = [];
}
