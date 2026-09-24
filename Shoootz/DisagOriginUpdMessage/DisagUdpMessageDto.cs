using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shoootz.DisagOriginUpdMessage;

public class DisagUdpMessageDto : DisagStructureBaseDto
{
    public DisagMessageType MessageType { get; set; }

    public DisagMessageVerb MessageVerb { get; set; }

    public bool Sequential { get; set; }

    public object? Ranges { get; set; }

    [JsonConverter(typeof(DisagStructureBaseListConverter))]
    public List<DisagStructureBaseDto>? Objects { get; set; }
}
