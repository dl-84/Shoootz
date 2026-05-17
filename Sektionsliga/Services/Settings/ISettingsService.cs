using Sektionsliga.Models;

namespace Sektionsliga.Services.Settings;

public interface ISettingsService
{
    AppSettingsModel Load();

    void Save(AppSettingsModel appSettings);
}
