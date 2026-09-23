using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Controls.InfoDialog;
using Controls.InfoDialog.Enum;
using Controls.WaitingDialog;
using Shoootz.Resources.Lang;
using Shoootz.Services.Localization;
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

    private void OnConnectDbIconPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is ConnectionViewModel viewModel)
        {
            viewModel.ConnectDbCommand.Execute(null);
        }
    }

    private void OnSaveIconPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is ConnectionViewModel viewModel)
        {
            viewModel.SaveCommand.Execute(null);
        }
    }

    private void OnDisconnectDbIconPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is ConnectionViewModel viewModel)
        {
            viewModel.DisconnectDbCommand.Execute(null);
        }
    }

    private void OnInitializeDbIconPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is ConnectionViewModel viewModel)
        {
            viewModel.InitializeDbCommand.Execute(null);
        }
    }

    private void OnTestDbConnectionIconPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is ConnectionViewModel viewModel)
        {
            viewModel.TestDbConnectionCommand.Execute(null);
        }
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
                DialogTitle = LocalizationService.Instance[Messages.InitializeDb],
                IconType = success ? IconType.Info : IconType.Warning,
                Message = success
                    ? LocalizationService.Instance[Messages.DbInitializeSuccessful]
                    : errorMessage ?? LocalizationService.Instance[Messages.ConnectionFailed],
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
                DialogTitle = LocalizationService.Instance[Messages.TestConnection],
                IconType = success ? IconType.Info : IconType.Warning,
                Message = success
                    ? LocalizationService.Instance[Messages.ConnectionSuccessful]
                    : errorMessage ?? LocalizationService.Instance[Messages.ConnectionFailed],
            }.ShowDialog(window);
        }
        finally
        {
            mainWindowViewModel?.IsDialogOpen = false;
        }
    }
}
