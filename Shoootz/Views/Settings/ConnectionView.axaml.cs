using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Controls.ContentDialog;
using Controls.InfoDialog;
using Controls.InfoDialog.Enum;
using Controls.JsonEditor;
using Controls.WaitingDialog;
using Shoootz.Resources.Lang;
using Shoootz.Services.Dialog;
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

    private static ContentDialog BuildConfirmDialog(string title, string message)
    {
        return new ContentDialog
        {
            DialogContent = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
            DialogTitle = title,
            IsDanger = true,
            SizeToContent = SizeToContent.Height,
            SubmitText = LocalizationService.Instance[Messages.Delete],
            Width = 460,
        };
    }

    private static JsonEditorControl BuildTextEditor(string content)
    {
        return new JsonEditorControl { IsReadOnly = true, Text = content };
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
            viewModel.DeleteAllCompleted += OnDeleteAllCompleted;
            viewModel.DeleteAllStarted += OnDeleteAllStarted;
            viewModel.DeleteSettingsFileRequested += OnDeleteSettingsFileRequested;
            viewModel.DeleteSettingsFolderRequested += OnDeleteSettingsFolderRequested;
            viewModel.ReadSettingsFileFailed += OnReadSettingsFileFailed;
            viewModel.SettingsContentRequested += OnSettingsContentRequested;
        }
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

    private void OnDeleteAllCompleted(bool success, string? errorMessage)
    {
        if (TopLevel.GetTopLevel(this) is Window { DataContext: MainWindowViewModel mainWindowViewModel })
        {
            mainWindowViewModel.IsDialogOpen = false;
        }

        _waitingDialog?.Close();
        _waitingDialog = null;

        _ = OpenDeleteAllDialogAsync(success, errorMessage);
    }

    private void OnDeleteAllStarted()
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
            DialogTitle = LocalizationService.Instance[Messages.DeleteAll],
            Message = LocalizationService.Instance[Messages.DeleteAllInProgress],
        };

        _ = _waitingDialog.ShowDialog(window);
    }

    private void OnDeleteSettingsFileRequested()
    {
        _ = OpenConfirmDialogAsync(
            LocalizationService.Instance[Messages.DeleteSettingsFile],
            LocalizationService.Instance[Messages.ConfirmDeleteSettingsFileMessage],
            () =>
            {
                if (DataContext is ConnectionViewModel viewModel)
                {
                    viewModel.ExecuteDeleteSettingsFile();
                }

                return Task.CompletedTask;
            }
        );
    }

    private void OnDeleteSettingsFolderRequested()
    {
        _ = OpenConfirmDialogAsync(
            LocalizationService.Instance[Messages.DeleteAll],
            LocalizationService.Instance[Messages.ConfirmDeleteAllMessage],
            () =>
                DataContext is ConnectionViewModel viewModel
                    ? viewModel.ExecuteDeleteSettingsFolderAsync()
                    : Task.CompletedTask
        );
    }

    private void OnReadSettingsFileFailed(string message)
    {
        _ = OpenReadSettingsFileFailedDialogAsync(message);
    }

    private void OnSettingsContentRequested(string content)
    {
        _ = OpenSettingsContentDialogAsync(content);
    }

    private async Task OpenConfirmDialogAsync(string title, string message, Func<Task> onConfirmed)
    {
        ContentDialog dialog = BuildConfirmDialog(title, message);
        bool? result = await ShowOverlayDialogAsync(window => dialog.ShowDialog<bool?>(window));

        if (result is true)
        {
            await onConfirmed();
        }
    }

    private async Task OpenConnectionTestDialogAsync(bool success, string? errorMessage)
    {
        await ShowOverlayDialogAsync(window =>
            new InfoDialog
            {
                DialogTitle = LocalizationService.Instance[Messages.TestConnection],
                IconType = success ? IconType.Info : IconType.Warning,
                Message = success
                    ? LocalizationService.Instance[Messages.ConnectionSuccessful]
                    : errorMessage ?? LocalizationService.Instance[Messages.ConnectionFailed],
            }.ShowDialog(window)
        );
    }

    private async Task OpenDbInitializeDialogAsync(bool success, string? errorMessage)
    {
        await ShowOverlayDialogAsync(window =>
            new InfoDialog
            {
                DialogTitle = LocalizationService.Instance[Messages.InitializeDb],
                IconType = success ? IconType.Info : IconType.Warning,
                Message = success
                    ? LocalizationService.Instance[Messages.DbInitializeSuccessful]
                    : errorMessage ?? LocalizationService.Instance[Messages.ConnectionFailed],
            }.ShowDialog(window)
        );
    }

    private async Task OpenDeleteAllDialogAsync(bool success, string? errorMessage)
    {
        await ShowOverlayDialogAsync(window =>
            new InfoDialog
            {
                DialogTitle = LocalizationService.Instance[Messages.DeleteAll],
                IconType = success ? IconType.Info : IconType.Warning,
                Message = success
                    ? LocalizationService.Instance[Messages.DeleteAllSuccessful]
                    : errorMessage ?? LocalizationService.Instance[Messages.ConnectionFailed],
            }.ShowDialog(window)
        );
    }

    private async Task OpenReadSettingsFileFailedDialogAsync(string message)
    {
        await DialogService.ShowErrorDialogAsync(this, message);
    }

    private async Task OpenSettingsContentDialogAsync(string content)
    {
        if (TopLevel.GetTopLevel(this) is not Window window)
        {
            return;
        }

        MainWindowViewModel? mainWindowViewModel = window.DataContext as MainWindowViewModel;

        try
        {
            mainWindowViewModel?.IsDialogOpen = true;

            await new ContentDialog
            {
                CopyText = content,
                DialogContent = BuildTextEditor(content),
                DialogTitle = LocalizationService.Instance[Messages.Settings],
                Height = 650,
                Width = 1000,
            }.ShowDialog(window);
        }
        finally
        {
            mainWindowViewModel?.IsDialogOpen = false;
        }
    }

    private async Task ShowOverlayDialogAsync(Func<Window, Task> showDialog)
    {
        await ShowOverlayDialogAsync<object?>(async window =>
        {
            await showDialog(window);
            return null;
        });
    }

    private async Task<TResult?> ShowOverlayDialogAsync<TResult>(Func<Window, Task<TResult>> showDialog)
    {
        if (TopLevel.GetTopLevel(this) is not Window window)
        {
            return default;
        }

        MainWindowViewModel? mainWindowViewModel = window.DataContext as MainWindowViewModel;

        try
        {
            mainWindowViewModel?.IsDialogOpen = true;
            return await showDialog(window);
        }
        finally
        {
            mainWindowViewModel?.IsDialogOpen = false;
        }
    }
}
