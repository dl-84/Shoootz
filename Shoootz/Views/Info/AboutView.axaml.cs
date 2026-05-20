using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;

namespace Shoootz.Views.Info;

/// <inheritdoc />
public partial class AboutView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AboutView"/> class.
    /// </summary>
    public AboutView()
    {
        InitializeComponent();
    }

    private void OnPointerLinkClicked(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control { Tag: string url })
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
    }
}
