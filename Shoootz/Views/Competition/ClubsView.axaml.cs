using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Controls.ContentDialog;
using Shoootz.Models.Club;
using Shoootz.Resources.Lang;
using Shoootz.Services.Dialog;
using Shoootz.Services.Localization;
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

    private static TextBox BuildClubNameTextBox(string clubName)
    {
        return new TextBox { Text = clubName, Width = 400 };
    }

    private static TextBlock BuildClubDeleteContent(string clubName)
    {
        return new TextBlock
        {
            Text = string.Format(LocalizationService.Instance[nameof(Messages.ConfirmDeleteClubMessage)], clubName),
            TextWrapping = TextWrapping.Wrap,
        };
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
        if (DataContext is not ClubsViewModel viewModel)
        {
            return;
        }

        ContentDialog dialog = new()
        {
            DialogContent = BuildClubDeleteContent(club.Name),
            DialogTitle = LocalizationService.Instance[nameof(Messages.Delete)],
            IsDanger = true,
            SizeToContent = SizeToContent.Height,
            SubmitText = LocalizationService.Instance[nameof(Messages.Delete)],
            Width = 460,
        };

        bool? result = await DialogService.ShowAsync(this, window => dialog.ShowDialog<bool?>(window));

        if (result is true)
        {
            await viewModel.ExecuteDeleteClubAsync(club);
        }
    }

    private async Task OpenCreateDialogAsync()
    {
        if (DataContext is not ClubsViewModel viewModel)
        {
            return;
        }

        TextBox dialogContent = BuildClubNameTextBox(string.Empty);
        bool? result = await DialogService.ShowAsync(
            this,
            window =>
                new ContentDialog
                {
                    SubmitText = LocalizationService.Instance[nameof(Messages.Add)],
                    DialogContent = dialogContent,
                    DialogTitle = LocalizationService.Instance[nameof(Messages.ClubName)],
                    SizeToContent = SizeToContent.WidthAndHeight,
                }.ShowDialog<bool?>(window)
        );

        if (result is true)
        {
            await viewModel.ExecuteCreateClubAsync(dialogContent.Text ?? string.Empty);
        }
    }

    private async Task OpenErrorDialogAsync(string message)
    {
        await DialogService.ShowErrorDialogAsync(this, message);
    }

    private async Task OpenRenameDialogAsync(ClubModel club)
    {
        if (DataContext is not ClubsViewModel viewModel)
        {
            return;
        }

        TextBox dialogContent = BuildClubNameTextBox(club.Name);
        bool? result = await DialogService.ShowAsync(
            this,
            window =>
                new ContentDialog
                {
                    SubmitText = LocalizationService.Instance[nameof(Messages.Save)],
                    DialogContent = dialogContent,
                    DialogTitle = LocalizationService.Instance[nameof(Messages.ClubName)],
                    SizeToContent = SizeToContent.WidthAndHeight,
                }.ShowDialog<bool?>(window)
        );

        if (result is true)
        {
            await viewModel.ExecuteRenameClubAsync(club, dialogContent.Text ?? string.Empty);
        }
    }
}
