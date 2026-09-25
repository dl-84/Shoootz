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

        return typeName?.ToUpperInvariant() switch
        {
            "SHOT" => element.Deserialize<DisagShotDto>(options)!,
            "SERIES" => element.Deserialize<DisagSeriesDto>(options)!,
            "RESULT" => element.Deserialize<DisagResultDto>(options)!,
            "LEAGUESTATSMODE1V1" => element.Deserialize<DisagLeagueStatsMode1v1Dto>(options)!,
            "LEAGUESTATS" => element.Deserialize<DisagLeagueStatsDto>(options)!,
            "CLUB" => element.Deserialize<DisagClubDto>(options)!,
            "TEAM" => element.Deserialize<DisagTeamDto>(options)!,
            "COMPETITION" => element.Deserialize<DisagCompetitionDto>(options)!,
            "MENUITEM" => element.Deserialize<DisagMenuItemDto>(options)!,
            _ => new DisagUnknownStructureDto { ObjectType = typeName, Raw = element.Clone() },
        };
    }
}
