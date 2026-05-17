using System;
using System.Globalization;
using System.Resources;

namespace Sektionsliga.Services.Localization;

public class LocalizationService : ILocalizationService
{
    private static readonly ResourceManager ResourceManager = new ResourceManager(
        "Sektionsliga.Resources.Messages",
        typeof(LocalizationService).Assembly
    );

    private CultureInfo _currentCulture = CultureInfo.CurrentUICulture;

    public event EventHandler? LanguageChanged;

    public string this[string key] => ResourceManager.GetString(key, _currentCulture) ?? key;

    public void SetLanguage(string cultureCode)
    {
        _currentCulture = new CultureInfo(cultureCode);
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
