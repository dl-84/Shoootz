using System.Text.Json.Serialization;
using Microsoft.VisualBasic.CompilerServices;

namespace Shoootz.Models.Udp;

internal class UdpMenuItemModel
{
    public string? MenuID { get; set; }

    public string? MenuItemName { get; set; }

    public string? MenuPointName { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ObjectType? ObjectType { get; set; }
}
