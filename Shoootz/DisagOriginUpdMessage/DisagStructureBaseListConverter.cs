using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shoootz.DisagOriginUpdMessage;

public class DisagStructureBaseListConverter : JsonConverter<List<DisagStructureBaseDto>>
{
    public override List<DisagStructureBaseDto>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        List<DisagStructureBaseDto> result = [];

        using JsonDocument document = JsonDocument.ParseValue(ref reader);

        foreach (JsonElement element in document.RootElement.EnumerateArray())
        {
            result.Add(ReadElement(element, options));
        }

        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        List<DisagStructureBaseDto>? value,
        JsonSerializerOptions options
    ) => throw new NotSupportedException("Not implemented");

    private static DisagStructureBaseDto ReadElement(JsonElement element, JsonSerializerOptions options)
    {
        string? typeName = element.TryGetProperty("ObjectType", out JsonElement objectTypeElement)
            ? objectTypeElement.GetString()
            : null;

        string rawText = element.GetRawText();

        return typeName switch
        {
            "Shot" => JsonSerializer.Deserialize<DisagShotDto>(rawText, options)!,
            "Series" => JsonSerializer.Deserialize<DisagSeriesDto>(rawText, options)!,
            "Result" => JsonSerializer.Deserialize<DisagResultDto>(rawText, options)!,
            "LeagueStatsMode1v1" => JsonSerializer.Deserialize<DisagLeagueStatsMode1v1Dto>(rawText, options)!,
            "LeagueStats" => JsonSerializer.Deserialize<DisagLeagueStatsDto>(rawText, options)!,
            "Club" => JsonSerializer.Deserialize<DisagClubDto>(rawText, options)!,
            "Team" => JsonSerializer.Deserialize<DisagTeamDto>(rawText, options)!,
            "Competition" => JsonSerializer.Deserialize<DisagCompetitionDto>(rawText, options)!,
            "MenuItem" => JsonSerializer.Deserialize<DisagMenuItemDto>(rawText, options)!,
            _ => new DisagUnknownStructureDto { ObjectType = typeName, Raw = element.Clone() },
        };
    }
}
