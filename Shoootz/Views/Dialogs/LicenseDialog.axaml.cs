using System.IO;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Shoootz.Views.Dialogs;

/// <inheritdoc />
public partial class LicenseDialog : Window
{
    public LicenseDialog()
    {
        InitializeComponent();

        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream stream = assembly.GetManifestResourceStream("Shoootz.LICENSE")!;
        using StreamReader reader = new StreamReader(stream);
        LicenseTextBlock.Text = reader.ReadToEnd();
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
