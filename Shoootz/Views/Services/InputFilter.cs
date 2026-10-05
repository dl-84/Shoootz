using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Shoootz.Views.Services;

internal static class InputFilter
{
    public static readonly AttachedProperty<string?> AllowedPatternProperty = AvaloniaProperty.RegisterAttached<
        TextBox,
        string?
    >("AllowedPattern", typeof(InputFilter));

    static InputFilter()
    {
        AllowedPatternProperty.Changed.AddClassHandler<TextBox>(OnAllowedPatternChanged);
    }

    public static string? GetAllowedPattern(TextBox element) => element.GetValue(AllowedPatternProperty);

    public static void SetAllowedPattern(TextBox element, string? value) =>
        element.SetValue(AllowedPatternProperty, value);

    private static void OnAllowedPatternChanged(TextBox textBox, AvaloniaPropertyChangedEventArgs e)
    {
        textBox.RemoveHandler(InputElement.TextInputEvent, OnTextInput);

        if (e.NewValue is string { Length: > 0 })
        {
            textBox.AddHandler(InputElement.TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
        }
    }

    private static void OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (sender is not TextBox textBox || e.Text is null)
        {
            return;
        }

        string? pattern = GetAllowedPattern(textBox);

        if (pattern is null)
        {
            return;
        }

        if (!Regex.IsMatch(e.Text, pattern))
        {
            e.Handled = true;
        }
    }
}
