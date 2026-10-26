using System;

namespace Shoootz.Common.Services.Localization;

public interface ILocalizationService
{
    event EventHandler? LanguageChanged;

    string this[string key] { get; }

    void SetLanguage(string cultureCode);
}
