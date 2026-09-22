using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Controls.ContentDialog;
using Controls.InfoDialog;
using Controls.InfoDialog.Enum;
using Controls.JsonEditor;
using Shoootz.Resources.Lang;
using Shoootz.Services.Localization;
using Shoootz.ViewModels;
using Shoootz.ViewModels.Settings;

namespace Shoootz.Views.Settings;

/// <inheritdoc/>
public partial class GeneralView : UserControl
{
    public GeneralView()
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

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is GeneralViewModel viewModel)
        {
            viewModel.DeleteSettingsFileRequested += OnDeleteSettingsFileRequested;
            viewModel.DeleteSettingsFolderRequested += OnDeleteSettingsFolderRequested;
            viewModel.SettingsContentRequested += OnSettingsContentRequested;
            viewModel.ReadSettingsFileFailed += OnReadSettingsFileFailed;
        }
    }

    private void OnDeleteSettingsFileRequested()
    {
        _ = OpenConfirmDialogAsync(
            LocalizationService.Instance[Messages.DeleteSettingsFile],
            LocalizationService.Instance[Messages.ConfirmDeleteSettingsFileMessage],
            () =>
            {
                if (DataContext is GeneralViewModel viewModel)
                {
                    viewModel.ExecuteDeleteSettingsFile();
                }
            }
        );
    }

    private void OnDeleteSettingsFolderRequested()
    {
        _ = OpenConfirmDialogAsync(
            LocalizationService.Instance[Messages.DeleteAll],
            LocalizationService.Instance[Messages.ConfirmDeleteAllMessage],
            () =>
            {
                if (DataContext is GeneralViewModel viewModel)
                {
                    viewModel.ExecuteDeleteSettingsFolder();
                }
            }
        );
    }

    private void OnSettingsContentRequested(string content)
    {
        _ = OpenSettingsContentDialogAsync(content);
    }

    private async Task OpenConfirmDialogAsync(string title, string message, Action onConfirmed)
    {
        if (TopLevel.GetTopLevel(this) is not Window window)
        {
            return;
        }

        ContentDialog dialog = BuildConfirmDialog(title, message);
        MainWindowViewModel? mainWindowViewModel = window.DataContext as MainWindowViewModel;

        try
        {
            mainWindowViewModel?.IsDialogOpen = true;

            bool? result = await dialog.ShowDialog<bool?>(window);

            if (result is true)
            {
                onConfirmed();
            }
        }
        finally
        {
            mainWindowViewModel?.IsDialogOpen = false;
        }
    }

    private void OnReadSettingsFileFailed(string message)
    {
        _ = OpenReadSettingsFileFailedDialogAsync(message);
    }

    private async Task OpenReadSettingsFileFailedDialogAsync(string message)
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
                DialogTitle = LocalizationService.Instance[Messages.Error],
                IconType = IconType.Error,
                Message = message,
            }.ShowDialog(window);
        }
        finally
        {
            mainWindowViewModel?.IsDialogOpen = false;
        }
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
}
