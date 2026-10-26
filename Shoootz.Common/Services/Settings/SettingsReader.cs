using System;
using System.IO;
using Result;
using Result.Types;
using Shoootz.Common.Models.Language.Error;
using Shoootz.Common.Services.App;

namespace Shoootz.Common.Services.Settings;

public static class SettingsReader
{
    public static Result<string, SettingsError> Read
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
                return new Error<SettingsError>(new SettingsError("ExceptionOnReadContent", exception.Message));
            }
        }
    }
}
