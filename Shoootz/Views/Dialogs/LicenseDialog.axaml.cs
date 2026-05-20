using System.IO;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Shoootz.Views.Dialogs;

/// <inheritdoc />
public partial class LicenseDialog : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseDialog"/> class.
    /// </summary>
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

    private void OnHeaderDrag(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}
