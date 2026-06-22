using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Controls.ConfirmDialog;
using Controls.ContentDialog;
using Controls.InfoDialog;
using Controls.InfoDialog.Enum;
using Controls.JsonEditor;
using Shoootz.Services.Localization;
using Shoootz.ViewModels;
using Shoootz.ViewModels.Settings;

namespace Shoootz.Views.Settings;

/// <inheritdoc />
public partial class GeneralView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneralView"/> class.
    /// </summary>
    public GeneralView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private static ConfirmDialog BuildConfirmDialog(string title, string message)
    {
        return new ConfirmDialog
        {
            AcceptText = LocalizationService.Instance["Delete"],
            CancelText = LocalizationService.Instance["Cancel"],
            DialogTitle = title,
            Message = message,
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
            LocalizationService.Instance["DeleteSettingsFile"],
            LocalizationService.Instance["ConfirmDeleteSettingsFileMessage"],
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
            LocalizationService.Instance["DeleteAll"],
            LocalizationService.Instance["ConfirmDeleteAllMessage"],
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

        ConfirmDialog dialog = BuildConfirmDialog(title, message);
        MainWindowViewModel? mainWindowViewModel = window.DataContext as MainWindowViewModel;

        try
        {
            if (mainWindowViewModel is not null)
            {
                mainWindowViewModel.IsDialogOpen = true;
            }

            bool? result = await dialog.ShowDialog<bool?>(window);

            if (result is true)
            {
                onConfirmed();
            }
        }
        finally
        {
            if (mainWindowViewModel is not null)
            {
                mainWindowViewModel.IsDialogOpen = false;
            }
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
            if (mainWindowViewModel is not null)
            {
                mainWindowViewModel.IsDialogOpen = true;
            }

            await new InfoDialog
            {
                CloseText = LocalizationService.Instance["Close"],
                DialogTitle = LocalizationService.Instance["Error"],
                IconType = IconType.Error,
                Message = message,
            }.ShowDialog(window);
        }
        finally
        {
            if (mainWindowViewModel is not null)
            {
                mainWindowViewModel.IsDialogOpen = false;
            }
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
                CloseText = LocalizationService.Instance["Close"],
                DialogContent = BuildTextEditor(content),
                DialogTitle = LocalizationService.Instance["ShowSettings"],
                Width = 1000,
            }.ShowDialog(window);
        }
        finally
        {
            mainWindowViewModel?.IsDialogOpen = false;
        }
    }
}
