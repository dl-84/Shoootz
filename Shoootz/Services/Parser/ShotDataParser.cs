using System;
using System.Text;
using System.Text.Json;
using Result;
using Result.Struct;
using Shoootz.Models.Shot;
using Shoootz.Resources.Lang;
using Shoootz.Services.Localization;

namespace Shoootz.Services.Parser;

internal class ShotDataParser(ILocalizationService localizationService) : IShotDataParser
{
    private static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public Result<Shot, ShotParseError> Run(byte[] data)
    {
        try
        {
            string rawData = Encoding.UTF8.GetString(data);
            Shot? shot = JsonSerializer.Deserialize<Shot>(rawData, _options);

            if (shot is null)
            {
                return new Error<ShotParseError>(
                    new ShotParseError(localizationService[nameof(Messages.ShotParseErrorNull)], rawData)
                );
            }

            return shot;
        }
        catch (Exception exception)
        {
            return new Error<ShotParseError>(new ShotParseError(exception.Message, Convert.ToBase64String(data)));
        }
    }
}
