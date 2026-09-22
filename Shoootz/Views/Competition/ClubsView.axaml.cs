using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Controls.ConfirmDialog;
using Controls.ContentDialog;
using Controls.InfoDialog;
using Controls.InfoDialog.Enum;
using Shoootz.Models.Club;
using Shoootz.Resources.Lang;
using Shoootz.Services.Localization;
using Shoootz.ViewModels;
using Shoootz.ViewModels.Competition;

namespace Shoootz.Views.Competition;

public partial class ClubsView : UserControl
{
    public ClubsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnAddIconPressed(object? sender, PointerPressedEventArgs e)
    {
        _ = OpenCreateDialogAsync();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ClubsViewModel viewModel)
        {
            viewModel.SaveClubFailed += OnSaveClubFailed;
        }
    }

    private void OnDeleteIconPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is ClubsViewModel { SelectedClub: { } club })
        {
            _ = OpenConfirmDeleteDialogAsync(club);
        }
    }

    private void OnEditIconPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is ClubsViewModel { SelectedClub: { } club })
        {
            _ = OpenRenameDialogAsync(club);
        }
    }

    private void OnSaveClubFailed(string message)
    {
        _ = OpenErrorDialogAsync(message);
    }

    private async Task OpenConfirmDeleteDialogAsync(ClubModel club)
    {
        if (TopLevel.GetTopLevel(this) is not Window window || DataContext is not ClubsViewModel viewModel)
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

            ConfirmDialog dialog = new()
            {
                AcceptText = LocalizationService.Instance[nameof(Messages.Delete)],
                CancelText = LocalizationService.Instance[nameof(Messages.Cancel)],
                DialogTitle = LocalizationService.Instance[nameof(Messages.Delete)],
                Message = club.Name,
            };

            bool? result = await dialog.ShowDialog<bool?>(window);

            if (result is true)
            {
                await viewModel.ExecuteDeleteClubAsync(club);
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

    private async Task OpenCreateDialogAsync()
    {
        if (TopLevel.GetTopLevel(this) is not Window window || DataContext is not ClubsViewModel viewModel)
        {
            return;
        }

        MainWindowViewModel? mainWindowViewModel = window.DataContext as MainWindowViewModel;
        TextBox nameTextBox = new();

        try
        {
            if (mainWindowViewModel is not null)
            {
                mainWindowViewModel.IsDialogOpen = true;
            }

            await new ContentDialog
            {
                CloseText = LocalizationService.Instance[nameof(Messages.Add)],
                DialogContent = nameTextBox,
                DialogTitle = LocalizationService.Instance[nameof(Messages.ClubName)],
            }.ShowDialog(window);

            await viewModel.ExecuteCreateClubAsync(nameTextBox.Text ?? string.Empty);
        }
        finally
        {
            if (mainWindowViewModel is not null)
            {
                mainWindowViewModel.IsDialogOpen = false;
            }
        }
    }

    private async Task OpenErrorDialogAsync(string message)
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
                CloseText = LocalizationService.Instance[nameof(Messages.Close)],
                DialogTitle = LocalizationService.Instance[nameof(Messages.Error)],
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

    private async Task OpenRenameDialogAsync(ClubModel club)
    {
        if (TopLevel.GetTopLevel(this) is not Window window || DataContext is not ClubsViewModel viewModel)
        {
            return;
        }

        MainWindowViewModel? mainWindowViewModel = window.DataContext as MainWindowViewModel;
        TextBox nameTextBox = new() { Text = club.Name };

        try
        {
            if (mainWindowViewModel is not null)
            {
                mainWindowViewModel.IsDialogOpen = true;
            }

            await new ContentDialog
            {
                CloseText = LocalizationService.Instance[nameof(Messages.Save)],
                DialogContent = nameTextBox,
                DialogTitle = LocalizationService.Instance[nameof(Messages.ClubName)],
            }.ShowDialog(window);

            await viewModel.ExecuteRenameClubAsync(club, nameTextBox.Text ?? string.Empty);
        }
        finally
        {
            if (mainWindowViewModel is not null)
            {
                mainWindowViewModel.IsDialogOpen = false;
            }
        }
    }
}
