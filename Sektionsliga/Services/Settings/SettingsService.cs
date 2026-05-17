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

    public AppSettingsDto Load()
    {
        if (!File.Exists(FilePath))
        {
            return new AppSettingsDto();
        }

        try
        {
            string content = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<AppSettingsDto>(content) ?? new AppSettingsDto();
        }
        catch
        {
            return new AppSettingsDto();
        }
    }

    public void Save(AppSettingsDto appSettings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(appSettings, JsonOptions));
    }
}
