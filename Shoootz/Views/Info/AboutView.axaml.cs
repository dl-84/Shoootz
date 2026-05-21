using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Shoootz.ViewModels;
using Shoootz.Views.Dialogs;

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

    private void OnMitLicenseClicked(object? sender, PointerPressedEventArgs e)
    {
        _ = OpenLicenseDialogAsync();
    }

    private async Task OpenLicenseDialogAsync()
    {
        if (TopLevel.GetTopLevel(this) is not Window window)
        {
            return;
        }

        MainWindowViewModel? vm = window.DataContext as MainWindowViewModel;

        try
        {
            if (vm is not null)
            {
                vm.IsDialogOpen = true;
            }

            await new LicenseDialog().ShowDialog(window);
        }
        finally
        {
            if (vm is not null)
            {
                vm.IsDialogOpen = false;
            }
        }
    }
}
