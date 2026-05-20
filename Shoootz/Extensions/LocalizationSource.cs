using System;
using System.ComponentModel;
using Shoootz.Services.Localization;

namespace Shoootz.Extensions;

internal class LocalizationSource : INotifyPropertyChanged
{
    private readonly string _key;

    public LocalizationSource(string key)
    {
        _key = key;
        LocalizationService.Instance.LanguageChanged += OnLanguageChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Value => LocalizationService.Instance[_key];

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }
}
