using System.Text.Json.Serialization;

namespace Shoootz.Models.Udp;

internal class UdpMenuItemModel
{
    public string? MenuID { get; set; }

    public string? MenuItemName { get; set; }

    public string? MenuPointName { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UdpObjectType? ObjectType { get; set; }
}
