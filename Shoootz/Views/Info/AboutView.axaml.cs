using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Controls.ContentDialog;
using Shoootz.Services.App;
using Shoootz.Services.Localization;
using Shoootz.ViewModels;

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
            vm?.IsDialogOpen = true;

            TextBlock licenseText = new TextBlock
            {
                Margin = new Thickness(24, 16, 24, 0),
                Text = content,
                TextWrapping = TextWrapping.Wrap,
            };

            await new ContentDialog
            {
                BackgroundColor = AppBrush.Background,
                CloseButtonColor = AppBrush.Green,
                CloseText = LocalizationService.Instance["Close"],
                DialogContent = licenseText,
                DialogTitle = "MIT License",
                PrimaryColor = AppBrush.Primary,
                TextColor = AppBrush.PrimaryForeground,
            }.ShowDialog(window);
        }
        finally
        {
            vm?.IsDialogOpen = false;
        }
    }
}
