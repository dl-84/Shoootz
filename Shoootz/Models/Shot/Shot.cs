using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shoootz.Models.Shot;

internal class Shot
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageType MessageType { get; set; }

    public string? MessageVerb { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ObjectType ObjectType { get; set; }

    public List<ShotData> Objects { get; set; } = [];

    public int Ranges { get; set; }

    public bool Sequential { get; set; }
}
