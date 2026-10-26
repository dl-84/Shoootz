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
using Shoootz.Common.Resources.Lang;
using Shoootz.Common.Services.Dialog;
using Shoootz.Common.Services.Localization;
using Shoootz.Common.ViewModels.Settings;

namespace Shoootz.Common.Views.Settings;

/// <inheritdoc/>
public partial class GeneralSettingsCard : UserControl
{
    private WaitingDialog? _waitingDialog;

    public GeneralSettingsCard()
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
            SubmitText = LocalizationService.Instance[nameof(Messages.Delete)],
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

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is GeneralSettingsViewModelBase viewModel)
        {
            viewModel.DeleteAllCompleted += OnDeleteAllCompleted;
            viewModel.DeleteAllStarted += OnDeleteAllStarted;
            viewModel.DeleteSettingsFileRequested += OnDeleteSettingsFileRequested;
            viewModel.DeleteSettingsFolderRequested += OnDeleteSettingsFolderRequested;
            viewModel.ReadSettingsFileFailed += OnReadSettingsFileFailed;
            viewModel.SettingsContentRequested += OnSettingsContentRequested;
        }
    }

    private void OnDeleteAllCompleted(bool success, string? errorMessage)
    {
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

        _waitingDialog = new WaitingDialog
        {
            DialogTitle = LocalizationService.Instance[nameof(Messages.DeleteAll)],
            Message = LocalizationService.Instance[nameof(Messages.DeleteAllInProgress)],
        };

        _ = _waitingDialog.ShowDialog(window);
    }

    private void OnDeleteSettingsFileRequested()
    {
        _ = OpenConfirmDialogAsync(
            LocalizationService.Instance[nameof(Messages.DeleteSettingsFile)],
            LocalizationService.Instance[nameof(Messages.ConfirmDeleteSettingsFileMessage)],
            () =>
            {
                if (DataContext is GeneralSettingsViewModelBase viewModel)
                {
                    viewModel.ExecuteDeleteSettingsFile();
                }

                return Task.CompletedTask;
            }
        );
    }

    private void OnDeleteSettingsFolderRequested()
    {
        if (DataContext is not GeneralSettingsViewModelBase viewModel)
        {
            return;
        }

        string message = LocalizationService.Instance[nameof(Messages.ConfirmDeleteAllMessage)];

        if (viewModel.HasAdditionalDeleteFolderCleanup)
        {
            message += $"\n\n{LocalizationService.Instance[nameof(Messages.ConfirmDeleteAllAdditionalDataHint)]}";
        }

        _ = OpenConfirmDialogAsync(
            LocalizationService.Instance[nameof(Messages.DeleteAll)],
            message,
            () => viewModel.ExecuteDeleteSettingsFolderAsync()
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
        bool? result = await DialogService.ShowAsync(this, window => dialog.ShowDialog<bool?>(window));

        if (result is true)
        {
            await onConfirmed();
        }
    }

    private async Task OpenDeleteAllDialogAsync(bool success, string? errorMessage)
    {
        await DialogService.ShowAsync(
            this,
            window =>
                new InfoDialog
                {
                    DialogTitle = LocalizationService.Instance[nameof(Messages.DeleteAll)],
                    IconType = success ? IconType.Info : IconType.Warning,
                    Message = success
                        ? LocalizationService.Instance[nameof(Messages.DeleteAllSuccessful)]
                        : errorMessage ?? LocalizationService.Instance[nameof(Messages.Error)],
                }.ShowDialog(window)
        );
    }

    private async Task OpenReadSettingsFileFailedDialogAsync(string message)
    {
        await DialogService.ShowErrorDialogAsync(this, message);
    }

    private async Task OpenSettingsContentDialogAsync(string content)
    {
        await DialogService.ShowAsync(
            this,
            window =>
                new ContentDialog
                {
                    CopyText = content,
                    DialogContent = BuildTextEditor(content),
                    DialogTitle = LocalizationService.Instance[nameof(Messages.Settings)],
                    Height = 650,
                    Width = 1000,
                }.ShowDialog(window)
        );
    }
}
