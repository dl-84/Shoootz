using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Shoootz.Common.Services.Localization;

public class LocalizationService(string resourceBaseName, Assembly resourceAssembly) : ILocalizationService
{
    private readonly ResourceManager _resourceManager = new(resourceBaseName, resourceAssembly);

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
