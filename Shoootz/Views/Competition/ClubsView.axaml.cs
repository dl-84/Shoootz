using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Controls.ContentDialog;
using Controls.FormField;
using Shoootz.Common.Resources.Lang;
using Shoootz.Common.Services.Dialog;
using Shoootz.Common.Services.Localization;
using Shoootz.Extensions;
using Shoootz.Models.Club;
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
                isEdit: false,
                onConfirmed: name => viewModel.ExecuteCreateClubAsync(name)
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
                isEdit: true,
                onConfirmed: name => viewModel.ExecuteRenameClubAsync(club, name)
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
                isEdit: true,
                onConfirmed: name => viewModel.ExecuteRenameClubAsync(club, name)
            );
        }
    }

    private void OnSaveClubFailed(string message)
    {
        _ = DialogService.ShowErrorDialogAsync(this, message);
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
        bool isEdit,
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
                    DialogContent = dialogContent,
                    DialogTitle = dialogTitle,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    SubmitText = LocalizationService.Instance[submitTextKey],
                    SubmitValidator = () =>
                        dialogContent.SetError(
                            !string.IsNullOrWhiteSpace(dialogContent.Text),
                            nameof(Messages.ClubNameRequiredMessage)
                        ),
                }.ShowDialog<bool?>(window)
        );

        if (result is true)
        {
            await onConfirmed(dialogContent.Text ?? string.Empty);
        }
        else if (isEdit)
        {
            viewModel.SelectedClub = null;
        }
    }
}
