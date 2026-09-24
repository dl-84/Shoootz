using System;
using System.Text.Json.Serialization;

namespace Shoootz.DisagOriginUpdMessage;

public class DisagCompetitionDto : DisagStructureBaseDto
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public DisagEvaluation Evaluation { get; set; }

    [JsonConverter(typeof(DisagDateTimeConverter))]
    public DateTime DateTime { get; set; }

    public int ID { get; set; }
}
