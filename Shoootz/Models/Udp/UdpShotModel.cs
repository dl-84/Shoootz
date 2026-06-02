using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shoootz.Models.Udp;

internal class UdpShotModel
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UdpMessageType UdpMessageType { get; set; }

    public string? MessageVerb { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UdpObjectType? ObjectType { get; set; }

    public List<UdpShotDetailModel> Objects { get; set; } = [];

    public int Ranges { get; set; }

    public bool Sequential { get; set; }
}
