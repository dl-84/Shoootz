using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Controls.InfoDialog;
using Controls.InfoDialog.Enum;
using Shoootz.Services.Localization;
using Shoootz.ViewModels;

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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            RootBorder.CornerRadius = new CornerRadius(0);
        }
    }

    /// <inheritdoc />
    protected override void OnOpened(EventArgs eventArgs)
    {
        base.OnOpened(eventArgs);
        _ = OnOpenedAsync();
    }

    private async Task OnOpenedAsync()
    {
        if (DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        mainWindowViewModel.PendingMigrationsDetected += OnPendingMigrationsDetected;
        await mainWindowViewModel.CheckPendingMigrationsAsync();
    }

    private void OnPendingMigrationsDetected()
    {
        _ = OpenPendingMigrationsDialogAsync();
    }

    private async Task OpenPendingMigrationsDialogAsync()
    {
        if (DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        try
        {
            mainWindowViewModel.IsDialogOpen = true;

            await new InfoDialog
            {
                CloseText = LocalizationService.Instance["Close"],
                DialogTitle = LocalizationService.Instance["DbPendingMigrationsTitle"],
                IconType = IconType.Warning,
                Message = LocalizationService.Instance["DbPendingMigrations"],
            }.ShowDialog(this);
        }
        finally
        {
            mainWindowViewModel.IsDialogOpen = false;
        }
    }
}
