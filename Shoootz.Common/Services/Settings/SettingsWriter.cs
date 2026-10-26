using System;
using System.IO;
using System.Text.Json;
using Shoootz.Common.Services.App;

namespace Shoootz.Common.Services.Settings;

public class SettingsWriter<TSettings> : ISettingsWriter<TSettings>
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    public event Action<TSettings>? SettingsSaved;

    public void DeleteSettingsFile()
    {
        if (File.Exists(AppPath.SettingsFile))
        {
            File.Delete(AppPath.SettingsFile);
        }
    }

    public void DeleteSettingsFolder()
    {
        if (Directory.Exists(AppPath.AppDataBase))
        {
            Directory.Delete(AppPath.AppDataBase, true);
        }
    }

    public void Save(TSettings? settings)
    {
        if (settings is null)
        {
            return;
        }

        SaveRaw(JsonSerializer.Serialize(settings, _jsonSerializerOptions));
        SettingsSaved?.Invoke(settings);
    }

    public void SaveRaw(string content)
    {
        if (!Directory.Exists(AppPath.AppDataBase))
        {
            Directory.CreateDirectory(AppPath.AppDataBase);
        }

        File.WriteAllText(AppPath.SettingsFile, content);
    }
}
