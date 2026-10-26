using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Shoootz.Common.Services.Localization;

namespace Shoootz.Common.Extensions;

public class LocalizationExtension(string key) : MarkupExtension
{
    public bool ToUpper { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding(nameof(LocalizationSourceAdapter.Value))
        {
            Source = new LocalizationSourceAdapter(key, ToUpper),
            Mode = BindingMode.OneWay,
        };
    }
}
