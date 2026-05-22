using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Shoootz.Views.Dialogs;

/// <inheritdoc />
public partial class LicenseDialog : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseDialog"/> class.
    /// </summary>
    /// <param name="content">The license text to display.</param>
    public LicenseDialog(string content)
    {
        InitializeComponent();
        LicenseTextBlock.Text = content;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
