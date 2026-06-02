using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic.CompilerServices;

namespace Shoootz.Models.Udp;

internal class UdpShot
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UdpMessageType UdpMessageType { get; set; }

    public string? MessageVerb { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ObjectType? ObjectType { get; set; }

    public List<UdpShotData> Objects { get; set; } = [];

    public int Ranges { get; set; }

    public bool Sequential { get; set; }
}
