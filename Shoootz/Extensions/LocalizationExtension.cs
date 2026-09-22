using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace Shoootz.Extensions;

public class LocalizationExtension(string key) : MarkupExtension
{
    public bool ToUpper { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding(nameof(LocalizationSource.Value))
        {
            Source = new LocalizationSource(key, ToUpper),
            Mode = BindingMode.OneWay,
        };
    }
}
