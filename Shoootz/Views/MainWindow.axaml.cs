using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Shoootz.Views;

/// <inheritdoc />
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        PropertyChanged += OnWindowPropertyChanged;
        bool isMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        UpdateMacMaximizeIcon(false);
        MacOsButtons.IsVisible = isMac;
        WindowControlButtons.IsVisible = !isMac;
    }

    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty)
        {
            WindowState newState = (WindowState)e.NewValue!;
            bool maximized = newState is WindowState.Maximized or WindowState.FullScreen;
            MaximizeIcon.Data = Geometry.Parse(maximized ? "M3,0 H11 V8 M0,3 H8 V11 H0 Z" : "M0,0 H11 V11 H0 Z");
            UpdateMacMaximizeIcon(maximized);
        }
    }

    private void UpdateMacMaximizeIcon(bool maximized)
    {
        SolidColorBrush iconBrush = new(Color.Parse("#0B6B1F"));
        if (maximized)
        {
            MacMaximizeBorder.Background = new SolidColorBrush(Color.Parse("#2EA44B"));
            MacMaximizeIcon.Width = 9;
            MacMaximizeIcon.Height = 9;
            MacMaximizeIcon.Data = Geometry.Parse("M0,4.5 L4.5,0 L4.5,4.5 Z M4.5,9 L9,4.5 L4.5,4.5 Z");
            MacMaximizeIcon.Fill = iconBrush;
            MacMaximizeIcon.Stroke = null;
        }
        else
        {
            MacMaximizeBorder.Background = new SolidColorBrush(Color.Parse("#35C759"));
            MacMaximizeIcon.Width = 7;
            MacMaximizeIcon.Height = 7;
            MacMaximizeIcon.Data = Geometry.Parse("M0.5,0.5 L5.5,0.5 L0.5,5.5 Z M6.5,6.5 L1.5,6.5 L6.5,1.5 Z");
            MacMaximizeIcon.Fill = iconBrush;
            MacMaximizeIcon.Stroke = null;
        }
    }

    private void OnTitleBarDrag(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void OnMinimizeClicked(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void OnMaximizeRestoreClicked(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnMacButtonsPointerEntered(object? sender, PointerEventArgs e)
    {
        MacCloseIcon.Opacity = 1;
        MacMinimizeIcon.Opacity = 1;
        MacMaximizeIcon.Opacity = 1;
    }

    private void OnMacButtonsPointerExited(object? sender, PointerEventArgs e)
    {
        MacCloseIcon.Opacity = 0;
        MacMinimizeIcon.Opacity = 0;
        MacMaximizeIcon.Opacity = 0;
    }

    private void OnMacCloseClicked(object? sender, PointerPressedEventArgs e)
    {
        Close();
    }

    private void OnMacMinimizeClicked(object? sender, PointerPressedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void OnMacMaximizeClicked(object? sender, PointerPressedEventArgs e)
    {
        WindowState = WindowState is WindowState.Maximized or WindowState.FullScreen
            ? WindowState.Normal
            : WindowState.Maximized;
        UpdateMacMaximizeIcon(WindowState is WindowState.Maximized or WindowState.FullScreen);
    }
}
