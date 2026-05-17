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

    private CultureInfo currentCulture = CultureInfo.CurrentUICulture;

    public event EventHandler? LanguageChanged;

    public string this[string key] => ResourceManager.GetString(key, currentCulture) ?? key;

    public void SetLanguage(string cultureCode)
    {
        currentCulture = new CultureInfo(cultureCode);
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
