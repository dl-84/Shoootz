using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Shoootz.Common.Extensions;

public static class InputFieldClearExtension
{
    public static readonly AttachedProperty<bool> IsEnabledProperty = AvaloniaProperty.RegisterAttached<TextBox, bool>(
        "IsEnabled",
        typeof(InputFieldClearExtension)
    );

    static InputFieldClearExtension()
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

        Path path = new() { Height = 12, Width = 12 };
        path.Bind(Path.DataProperty, new DynamicResourceExtension("XOutlineIcon"));

        Border icon = new()
        {
            Child = path,
            Classes = { "icon-action", "clear-icon" },
            Margin = new Thickness(0, 0, 4, 0),
            Padding = new Thickness(6),
        };

        icon.PointerPressed += (_, _) => textBox.Text = string.Empty;

        textBox.InnerRightContent = icon;
    }
}
