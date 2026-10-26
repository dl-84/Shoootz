using System;

namespace Shoootz.Common.Services.Settings;

public interface ISettingsWriter<TSettings>
{
    event Action<TSettings>? SettingsSaved;

    void DeleteSettingsFile();

    void DeleteSettingsFolder();

    void Save(TSettings? settings);

    void SaveRaw(string content);
}
