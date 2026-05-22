using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace Shoootz.Services.Localization;

internal class LocalizationService : ILocalizationService
{
    private static readonly ResourceManager _resourceManager = new ResourceManager(
        "Shoootz.Resources.Lang.Messages",
        typeof(LocalizationService).Assembly
    );

    private readonly List<WeakReference<EventHandler>> _handlers = [];

    private CultureInfo _currentCulture = CultureInfo.CurrentUICulture;

    public LocalizationService()
    {
        Instance = this;
    }

    public event EventHandler? LanguageChanged
    {
        add
        {
            if (value is not null)
            {
                _handlers.Add(new WeakReference<EventHandler>(value));
            }
        }
        remove { }
    }

    public static LocalizationService Instance { get; private set; } = null!;

    public string this[string key] => _resourceManager.GetString(key, _currentCulture) ?? key;

    public void SetLanguage(string cultureCode)
    {
        _currentCulture = new CultureInfo(cultureCode);
        _handlers.RemoveAll(weakReference => !weakReference.TryGetTarget(out _));

        foreach (WeakReference<EventHandler> weakReference in _handlers)
        {
            if (weakReference.TryGetTarget(out EventHandler? handler))
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
