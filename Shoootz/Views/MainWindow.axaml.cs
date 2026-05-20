using Avalonia.Controls;
using Avalonia.Input;

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
    }

    private void OnTitleBarDrag(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}
