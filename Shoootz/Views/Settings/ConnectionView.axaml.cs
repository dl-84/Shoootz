using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Controls.InfoDialog;
using Controls.InfoDialog.Enum;
using Controls.WaitingDialog;
using Shoootz.Common.Resources.Lang;
using Shoootz.Common.Services.Dialog;
using Shoootz.Common.Services.Localization;
using Shoootz.ViewModels;
using Shoootz.ViewModels.Settings;

namespace Shoootz.Views.Settings;

/// <inheritdoc/>
public partial class ConnectionView : UserControl
{
    private WaitingDialog? _waitingDialog;

    public ConnectionView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnCommandIconPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is PathIcon { Tag: ICommand command })
        {
            command.Execute(null);
        }
    }

    private void OnConnectionTestCompleted(bool success, string? errorMessage)
    {
        _ = OpenConnectionTestDialogAsync(success, errorMessage);
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

    private void OnDbInitializeCompleted(bool success, string? errorMessage)
    {
        if (TopLevel.GetTopLevel(this) is Window { DataContext: MainWindowViewModel mainWindowViewModel })
        {
            mainWindowViewModel.IsDialogOpen = false;

            if (success)
            {
                _ = mainWindowViewModel.CheckDbConnectionAsync();
            }
        }

        _waitingDialog?.Close();
        _waitingDialog = null;

        _ = OpenDbInitializeDialogAsync(success, errorMessage);
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
            DialogTitle = LocalizationService.Instance[Messages.InitializeDb],
            Message = LocalizationService.Instance[Messages.DbInitializing],
        };

        _ = _waitingDialog.ShowDialog(window);
    }

    private async Task OpenConnectionTestDialogAsync(bool success, string? errorMessage) =>
        await OpenResultDialogAsync(
            LocalizationService.Instance[Messages.TestConnection],
            success,
            LocalizationService.Instance[Messages.ConnectionSuccessful],
            errorMessage
        );

    private async Task OpenDbInitializeDialogAsync(bool success, string? errorMessage) =>
        await OpenResultDialogAsync(
            LocalizationService.Instance[Messages.InitializeDb],
            success,
            LocalizationService.Instance[Messages.DbInitializeSuccessful],
            errorMessage
        );

    private async Task OpenResultDialogAsync(string title, bool success, string successMessage, string? errorMessage)
    {
        await DialogService.ShowAsync(
            this,
            window =>
                new InfoDialog
                {
                    DialogTitle = title,
                    IconType = success ? IconType.Info : IconType.Warning,
                    Message = success
                        ? successMessage
                        : errorMessage ?? LocalizationService.Instance[Messages.ConnectionFailed],
                }.ShowDialog(window)
        );
    }
}
