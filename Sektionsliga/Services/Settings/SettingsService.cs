using System;
using System.IO;
using System.Text.Json;
using Sektionsliga.Models;

namespace Sektionsliga.Services.Settings;

public class SettingsService : ISettingsService
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        AppDomain.CurrentDomain.FriendlyName,
        "appsettings.json"
    );

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = true };

    public AppSettingsModel Load()
    {
        if (!File.Exists(FilePath))
        {
            return new AppSettingsModel();
        }

        try
        {
            string content = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<AppSettingsModel>(content) ?? new AppSettingsModel();
        }
        catch
        {
            return new AppSettingsModel();
        }
    }

    public void Save(AppSettingsModel appSettings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(appSettings, JsonOptions));
    }
}
