using System;
using System.Globalization;
using System.Resources;

namespace Shoootz.Services.Localization;

internal class LocalizationService : ILocalizationService
{
    private static readonly ResourceManager _resourceManager = new ResourceManager(
        "Shoootz.Resources.Lang.Messages",
        typeof(LocalizationService).Assembly
    );

    private CultureInfo _currentCulture = CultureInfo.CurrentUICulture;

    public event EventHandler? LanguageChanged;

    public static ILocalizationService Instance { get; private set; } = null!;

    public string this[string key] => _resourceManager.GetString(key, _currentCulture) ?? key;

    public static void Register(ILocalizationService instance)
    {
        Instance = instance;
    }

    public void SetLanguage(string cultureCode)
    {
        _currentCulture = new CultureInfo(cultureCode);
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
