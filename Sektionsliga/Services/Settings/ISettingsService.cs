using Sektionsliga.Models;

namespace Sektionsliga.Services.Settings;

public interface ISettingsService
{
    AppSettingsDto Load();

    void Save(AppSettingsDto appSettings);
}
