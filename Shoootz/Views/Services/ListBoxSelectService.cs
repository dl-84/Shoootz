using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Shoootz.Views.Services;

internal static class ListBoxSelectService
{
    public static readonly AttachedProperty<bool> DeselectOnClickProperty = AvaloniaProperty.RegisterAttached<
        ListBox,
        bool
    >("DeselectOnClick", typeof(ListBoxSelectService));

    static ListBoxSelectService()
    {
        DeselectOnClickProperty.Changed.AddClassHandler<ListBox>(OnDeselectOnClickChanged);
    }

    public static bool GetDeselectOnClick(ListBox element) => element.GetValue(DeselectOnClickProperty);

    public static void SetDeselectOnClick(ListBox element, bool value) =>
        element.SetValue(DeselectOnClickProperty, value);

    private static void OnDeselectOnClickChanged(ListBox listBox, AvaloniaPropertyChangedEventArgs e)
    {
        listBox.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);

        if (e.NewValue is true)
        {
            listBox.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        }
    }

    private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not ListBox listBox || e.ClickCount != 1)
        {
            return;
        }

        if ((e.Source as Visual)?.FindAncestorOfType<ListBoxItem>() is not { } item)
        {
            return;
        }

        if (ReferenceEquals(item.DataContext, listBox.SelectedItem))
        {
            listBox.SelectedItem = null;
            e.Handled = true;
        }
    }
}
