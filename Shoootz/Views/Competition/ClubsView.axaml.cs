using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Controls.ContentDialog;
using Controls.FormField;
using Shoootz.Extensions;
using Shoootz.Models.Club;
using Shoootz.Resources.Lang;
using Shoootz.Services.Dialog;
using Shoootz.Services.Localization;
using Shoootz.ViewModels.Competition;
using Shoootz.Views.Extensions;

namespace Shoootz.Views.Competition;

/// <inheritdoc/>
public partial class ClubsView : UserControl
{
    public ClubsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private static FormFieldControl BuildClubNameField(string clubName)
    {
        return new FormFieldControl
        {
            Label = LocalizationService.Instance[nameof(Messages.Name)],
            MaxLength = 255,
            Text = clubName,
            Width = 400,
        };
    }

    private void OnAddIconPressed(object? sender, EventArgs e)
    {
        if (DataContext is ClubsViewModel viewModel)
        {
            _ = OpenClubNameDialogAsync(
                string.Empty,
                LocalizationService.Instance[nameof(Messages.AddClubTitle)],
                nameof(Messages.Add),
                name => viewModel.ExecuteCreateClubAsync(name)
            );
        }
    }

    private void OnClubRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (
            DataContext is ClubsViewModel viewModel
            && (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>() is { DataContext: ClubModel club }
        )
        {
            _ = OpenClubNameDialogAsync(
                club.Name,
                string.Format(LocalizationService.Instance[nameof(Messages.EditClubTitle)], club.Id),
                nameof(Messages.Save),
                name => viewModel.ExecuteRenameClubAsync(club, name)
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

    private void OnDeleteIconPressed(object? sender, EventArgs e)
    {
        if (DataContext is ClubsViewModel { SelectedClub: { } club })
        {
            _ = OpenConfirmDeleteDialogAsync(club);
        }
    }

    private void OnEditIconPressed(object? sender, EventArgs e)
    {
        if (DataContext is ClubsViewModel { SelectedClub: { } club } viewModel)
        {
            _ = OpenClubNameDialogAsync(
                club.Name,
                string.Format(LocalizationService.Instance[nameof(Messages.EditClubTitle)], club.Id),
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
            DialogContent = LocalizationService
                .Instance[nameof(Messages.ConfirmDeleteClubMessage)]
                .Highlight(club.Name),
            DialogTitle = LocalizationService.Instance[nameof(Messages.DeleteClubTitle)],
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

    private async Task OpenClubNameDialogAsync(
        string initialName,
        string dialogTitle,
        string submitTextKey,
        Func<string, Task> onConfirmed
    )
    {
        if (DataContext is not ClubsViewModel viewModel)
        {
            return;
        }

        FormFieldControl dialogContent = BuildClubNameField(initialName);

        bool? result = await DialogService.ShowAsync(
            this,
            window =>
                new ContentDialog
                {
                    SubmitText = LocalizationService.Instance[submitTextKey],
                    SubmitValidator = () =>
                        dialogContent.SetError(
                            !string.IsNullOrWhiteSpace(dialogContent.Text),
                            nameof(Messages.ClubNameRequiredMessage)
                        ),
                    DialogContent = dialogContent,
                    DialogTitle = dialogTitle,
                    SizeToContent = SizeToContent.WidthAndHeight,
                }.ShowDialog<bool?>(window)
        );

        if (result is true)
        {
            await onConfirmed(dialogContent.Text ?? string.Empty);
        }
        else
        {
            viewModel.SelectedClub = null;
        }
    }

    private async Task OpenErrorDialogAsync(string message)
    {
        await DialogService.ShowErrorDialogAsync(this, message);
    }
}
