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
        if (DataContext is ClubsViewModel viewModel)
        {
            _ = OpenClubNameDialogAsync(
                string.Empty,
                nameof(Messages.Add),
                name => viewModel.ExecuteCreateClubAsync(name)
            );
        }
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
        if (DataContext is ClubsViewModel { SelectedClub: { } club } viewModel)
        {
            _ = OpenClubNameDialogAsync(
                club.Name,
                nameof(Messages.Save),
                name => viewModel.ExecuteRenameClubAsync(club, name)
            );
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

    private async Task OpenClubNameDialogAsync(string initialName, string submitTextKey, Func<string, Task> onConfirmed)
    {
        TextBox dialogContent = BuildClubNameTextBox(initialName);
        bool? result = await DialogService.ShowAsync(
            this,
            window =>
                new ContentDialog
                {
                    SubmitText = LocalizationService.Instance[submitTextKey],
                    DialogContent = dialogContent,
                    DialogTitle = LocalizationService.Instance[nameof(Messages.ClubName)],
                    SizeToContent = SizeToContent.WidthAndHeight,
                }.ShowDialog<bool?>(window)
        );

        if (result is true)
        {
            await onConfirmed(dialogContent.Text ?? string.Empty);
        }
    }

    private async Task OpenErrorDialogAsync(string message)
    {
        await DialogService.ShowErrorDialogAsync(this, message);
    }
}
