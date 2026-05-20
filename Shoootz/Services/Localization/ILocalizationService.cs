using System;

namespace Shoootz.Services.Localization;

internal interface ILocalizationService
{
    event EventHandler? LanguageChanged;

    string this[string key] { get; }

    void SetLanguage(string cultureCode);
}
