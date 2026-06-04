using System;
using System.ComponentModel;
using Shoootz.Services.Localization;

namespace Shoootz.Extensions;

internal class LocalizationSource : INotifyPropertyChanged
{
    private readonly string _key;

    private readonly bool _toUpper;

    public LocalizationSource(string key, bool toUpper = false)
    {
        _key = key;
        _toUpper = toUpper;
        LocalizationService.Instance.LanguageChanged += OnLanguageChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Value
    {
        get
        {
            string value = LocalizationService.Instance[_key];
            return _toUpper ? value.ToUpper() : value;
        }
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }
}
