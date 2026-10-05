using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Shoootz.Views.Settings;

namespace Shoootz.Views.Extensions;

public class LocalizationExtension(string key) : MarkupExtension
{
    public bool ToUpper { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding(nameof(LocalizationSourceService.Value))
        {
            Source = new LocalizationSourceService(key, ToUpper),
            Mode = BindingMode.OneWay,
        };
    }
}
