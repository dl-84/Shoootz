using System;
using System.IO;
using Result;
using Result.Types;
using Shoootz.Models.Error;
using Shoootz.Models.Settings;
using Shoootz.Services.App;

namespace Shoootz.Services.Settings.Read;

internal static class SettingsReader
{
    internal static Result<string, SettingsError> Read
    {
        get
        {
            try
            {
                string result = File.ReadAllText(AppPath.SettingsFile);
                return result;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return new Error<SettingsError>(
                    new SettingsError(SettingsPropertyType.ExceptionOnReadContent, exception.Message)
                );
            }
        }
    }
}
