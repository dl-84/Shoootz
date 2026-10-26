using System;
using System.ComponentModel;

namespace Shoootz.Common.Services.Localization;

internal class LocalizationSourceAdapter : INotifyPropertyChanged
{
    private readonly bool _toUpper;

    public LocalizationSourceAdapter(string key, bool toUpper = false)
    {
        Value = key;
        _toUpper = toUpper;
        LocalizationService.Instance.LanguageChanged += OnLanguageChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Value
    {
        get
        {
            string value = LocalizationService.Instance[field];
            return _toUpper ? value.ToUpper() : value;
        }
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }
}
