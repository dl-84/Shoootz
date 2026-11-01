using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Shoootz.Common.Extensions;

public static class ClearButton
{
    public static readonly AttachedProperty<bool> IsEnabledProperty = AvaloniaProperty.RegisterAttached<TextBox, bool>(
        "IsEnabled",
        typeof(ClearButton)
    );

    static ClearButton()
    {
        IsEnabledProperty.Changed.AddClassHandler<TextBox>(OnIsEnabledChanged);
    }

    public static bool GetIsEnabled(TextBox element) => element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(TextBox element, bool value) => element.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(TextBox textBox, AvaloniaPropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.NewValue is not true)
        {
            textBox.InnerRightContent = null;
            return;
        }

        PathIcon icon = new()
        {
            Classes = { "icon-action", "clear-icon" },
            Height = 12,
            Margin = new Thickness(0, 0, 10, 0),
            Width = 12,
        };

        icon.Bind(PathIcon.DataProperty, new DynamicResourceExtension("CloseIcon"));
        icon.PointerPressed += (_, _) => textBox.Text = string.Empty;

        textBox.InnerRightContent = icon;
    }
}
