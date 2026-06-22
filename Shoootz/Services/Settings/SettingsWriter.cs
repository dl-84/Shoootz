using System;
using System.IO;
using System.Text.Json;
using Shoootz.Models.Settings;
using Shoootz.Services.App;

namespace Shoootz.Services.Settings;

internal class SettingsWriter : ISettingsWriter
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    public event Action<SettingsModel>? SettingsSaved;

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

    public void Save(SettingsModel? settings)
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
