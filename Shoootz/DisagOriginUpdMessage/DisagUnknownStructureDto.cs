using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shoootz.DisagOriginUpdMessage;

public class DisagUnknownStructureDto : DisagStructureBaseDto
{
    [JsonIgnore]
    public JsonElement? Raw { get; set; }
}
