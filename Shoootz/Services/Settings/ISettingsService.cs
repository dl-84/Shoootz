using System.Collections.Generic;
using Result;
using Shoootz.Models.Settings;

namespace Shoootz.Services.Settings;

internal interface ISettingsService
{
    void DeleteSettingsFile();

    void DeleteSettingsFolder();

    Result<SettingsModel, List<SettingsError>> Load();

    string LoadRaw();

    void Save(SettingsModel? settings);

    void SaveRaw(string content);

    Result<SettingsModel, List<SettingsError>> Validate(string content);
}
