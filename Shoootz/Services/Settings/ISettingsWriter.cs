using System;
using Shoootz.Models.Settings;

namespace Shoootz.Services.Settings;

internal interface ISettingsWriter
{
    event Action<SettingsModel>? SettingsSaved;

    void DeleteSettingsFile();

    void DeleteSettingsFolder();

    void Save(SettingsModel? settings);

    void SaveRaw(string content);
}
