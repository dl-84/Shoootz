using System.Threading.Tasks;
using Avalonia.Controls;
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

    private void OnLicenseContentRequested(object? sender, string content)
    {
        _ = OpenLicenseDialogAsync(content);
    }

    private async Task OpenLicenseDialogAsync(string content)
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

            await new LicenseDialog(content).ShowDialog(window);
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
