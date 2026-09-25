using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Controls.ContentDialog;
using Shoootz.Resources.Lang;
using Shoootz.Services.Dialog;
using Shoootz.Services.Localization;

namespace Shoootz.Views.Info;

/// <inheritdoc/>
public partial class AboutView : UserControl
{
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
        TextBlock licenseText = new()
        {
            Margin = new Thickness(24, 16, 24, 0),
            Text = content,
            TextWrapping = TextWrapping.Wrap,
        };

        await DialogService.ShowAsync(
            this,
            window =>
                new ContentDialog
                {
                    DialogContent = licenseText,
                    DialogTitle = LocalizationService.Instance[Messages.MitLicense],
                    Height = 650,
                    Width = 800,
                }.ShowDialog(window)
        );
    }
}
