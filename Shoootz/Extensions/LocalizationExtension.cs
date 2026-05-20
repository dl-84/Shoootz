using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace Shoootz.Extensions;

/// <inheritdoc />
public class LocalizationExtension(string key) : MarkupExtension
{
    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding(nameof(LocalizationSource.Value))
        {
            Source = new LocalizationSource(key),
            Mode = BindingMode.OneWay,
        };
    }
}
