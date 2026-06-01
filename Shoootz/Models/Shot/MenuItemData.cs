using System.Text.Json.Serialization;

namespace Shoootz.Models.Shot;

internal class MenuItemData
{
    public string? MenuID { get; set; }

    public string? MenuItemName { get; set; }

    public string? MenuPointName { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ObjectType ObjectType { get; set; }
}
