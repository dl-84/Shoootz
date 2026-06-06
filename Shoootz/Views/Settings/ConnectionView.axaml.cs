using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Controls.InfoDialog;
using Controls.InfoDialog.Enum;
using Controls.WaitingDialog;
using Shoootz.Services.Localization;
using Shoootz.ViewModels;
using Shoootz.ViewModels.Settings;

namespace Shoootz.Views.Settings;

/// <inheritdoc />
public partial class ConnectionView : UserControl
{
    private WaitingDialog? _waitingDialog;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionView"/> class.
    /// </summary>
    public ConnectionView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ConnectionViewModel viewModel)
        {
            viewModel.ConnectionTestCompleted += OnConnectionTestCompleted;
            viewModel.DbInitializeCompleted += OnDbInitializeCompleted;
            viewModel.DbInitializeStarted += OnDbInitializeStarted;
        }
    }

    private void OnConnectionTestCompleted(bool success, string? errorMessage)
    {
        _ = OpenConnectionTestDialogAsync(success, errorMessage);
    }

    private void OnDbInitializeStarted()
    {
        if (TopLevel.GetTopLevel(this) is not Window window)
        {
            return;
        }

        if (window.DataContext is MainWindowViewModel mainWindowViewModel)
        {
            mainWindowViewModel.IsDialogOpen = true;
        }

        _waitingDialog = new WaitingDialog
        {
            DialogTitle = LocalizationService.Instance["InitializeDb"],
            Message = LocalizationService.Instance["DbInitializing"],
        };

        _ = _waitingDialog.ShowDialog(window);
    }

    private void OnDbInitializeCompleted(bool success, string? errorMessage)
    {
        if (TopLevel.GetTopLevel(this) is Window { DataContext: MainWindowViewModel mainWindowViewModel })
        {
            mainWindowViewModel.IsDialogOpen = false;

            if (success)
            {
                mainWindowViewModel.CheckDbConnection();
            }
        }

        _waitingDialog?.Close();
        _waitingDialog = null;

        _ = OpenDbInitializeDialogAsync(success, errorMessage);
    }

    private async Task OpenDbInitializeDialogAsync(bool success, string? errorMessage)
    {
        if (TopLevel.GetTopLevel(this) is not Window window)
        {
            return;
        }

        MainWindowViewModel? mainWindowViewModel = window.DataContext as MainWindowViewModel;

        try
        {
            mainWindowViewModel?.IsDialogOpen = true;

            await new InfoDialog
            {
                CloseText = LocalizationService.Instance["Close"],
                DialogTitle = LocalizationService.Instance["InitializeDb"],
                IconType = success ? IconType.Info : IconType.Warning,
                Message = success
                    ? LocalizationService.Instance["DbInitializeSuccessful"]
                    : errorMessage ?? LocalizationService.Instance["ConnectionFailed"],
            }.ShowDialog(window);
        }
        finally
        {
            mainWindowViewModel?.IsDialogOpen = false;
        }
    }

    private async Task OpenConnectionTestDialogAsync(bool success, string? errorMessage)
    {
        if (TopLevel.GetTopLevel(this) is not Window window)
        {
            return;
        }

        MainWindowViewModel? mainWindowViewModel = window.DataContext as MainWindowViewModel;

        try
        {
            mainWindowViewModel?.IsDialogOpen = true;

            await new InfoDialog
            {
                CloseText = LocalizationService.Instance["Close"],
                DialogTitle = LocalizationService.Instance["TestConnection"],
                IconType = success ? IconType.Info : IconType.Warning,
                Message = success
                    ? LocalizationService.Instance["ConnectionSuccessful"]
                    : errorMessage ?? LocalizationService.Instance["ConnectionFailed"],
            }.ShowDialog(window);
        }
        finally
        {
            mainWindowViewModel?.IsDialogOpen = false;
        }
    }
}
