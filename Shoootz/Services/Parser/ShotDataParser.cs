using System;
using System.Text;
using System.Text.Json;
using Result;
using Result.Struct;
using Shoootz.Models.Shot;

namespace Shoootz.Services.Parser;

internal class ShotDataParser : IShotDataParser
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };

    public Result<Shot, ShotParseError> Run(byte[] data)
    {
        try
        {
            string json = Encoding.UTF8.GetString(data);
            Shot? message = JsonSerializer.Deserialize<Shot>(json, _options);

            if (message is null)
            {
                return new Error<ShotParseError>(new ShotParseError("Deserialization returned null."));
            }

            return message;
        }
        catch (Exception exception)
        {
            return new Error<ShotParseError>(new ShotParseError(exception.Message));
        }
    }
}
