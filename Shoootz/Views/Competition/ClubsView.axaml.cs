using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Controls.ContentDialog;
using Controls.InfoDialog;
using Controls.InfoDialog.Enum;
using Shoootz.Models.Club;
using Shoootz.Resources.Lang;
using Shoootz.Services.Localization;
using Shoootz.ViewModels;
using Shoootz.ViewModels.Competition;

namespace Shoootz.Views.Competition;

/// <inheritdoc/>
public partial class ClubsView : UserControl
{
    public ClubsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private static TextBox BuildClubNameTextBox(string text)
    {
        return new TextBox { Text = text, Width = 400 };
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
            mainWindowViewModel?.IsDialogOpen = true;

            ContentDialog dialog = new()
            {
                DialogContent = new TextBlock { Text = club.Name, TextWrapping = TextWrapping.Wrap },
                DialogTitle = LocalizationService.Instance[nameof(Messages.Delete)],
                IsDanger = true,
                SizeToContent = SizeToContent.Height,
                SubmitText = LocalizationService.Instance[nameof(Messages.Delete)],
                Width = 460,
            };

            bool? result = await dialog.ShowDialog<bool?>(window);

            if (result is true)
            {
                await viewModel.ExecuteDeleteClubAsync(club);
            }
        }
        finally
        {
            mainWindowViewModel?.IsDialogOpen = false;
        }
    }

    private async Task OpenCreateDialogAsync()
    {
        if (TopLevel.GetTopLevel(this) is not Window window || DataContext is not ClubsViewModel viewModel)
        {
            return;
        }

        MainWindowViewModel? mainWindowViewModel = window.DataContext as MainWindowViewModel;
        TextBox dialogContent = BuildClubNameTextBox(string.Empty);

        try
        {
            mainWindowViewModel?.IsDialogOpen = true;

            bool? result = await new ContentDialog
            {
                SubmitText = LocalizationService.Instance[nameof(Messages.Add)],
                DialogContent = dialogContent,
                DialogTitle = LocalizationService.Instance[nameof(Messages.ClubName)],
                SizeToContent = SizeToContent.WidthAndHeight,
            }.ShowDialog<bool?>(window);

            if (result is true)
            {
                await viewModel.ExecuteCreateClubAsync(dialogContent.Text ?? string.Empty);
            }
        }
        finally
        {
            mainWindowViewModel?.IsDialogOpen = false;
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
            mainWindowViewModel?.IsDialogOpen = true;

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
            mainWindowViewModel?.IsDialogOpen = false;
        }
    }

    private async Task OpenRenameDialogAsync(ClubModel club)
    {
        if (TopLevel.GetTopLevel(this) is not Window window || DataContext is not ClubsViewModel viewModel)
        {
            return;
        }

        MainWindowViewModel? mainWindowViewModel = window.DataContext as MainWindowViewModel;
        TextBox dialogContent = BuildClubNameTextBox(club.Name);

        try
        {
            mainWindowViewModel?.IsDialogOpen = true;

            bool? result = await new ContentDialog
            {
                SubmitText = LocalizationService.Instance[nameof(Messages.Save)],
                DialogContent = dialogContent,
                DialogTitle = LocalizationService.Instance[nameof(Messages.ClubName)],
                SizeToContent = SizeToContent.WidthAndHeight,
            }.ShowDialog<bool?>(window);

            if (result is true)
            {
                await viewModel.ExecuteRenameClubAsync(club, dialogContent.Text ?? string.Empty);
            }
        }
        finally
        {
            mainWindowViewModel?.IsDialogOpen = false;
        }
    }
}
