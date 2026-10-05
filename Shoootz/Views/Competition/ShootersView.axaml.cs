using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Controls.ContentDialog;
using Shoootz.Extensions;
using Shoootz.Models.Shooter;
using Shoootz.Resources.Lang;
using Shoootz.Services.Dialog;
using Shoootz.Services.Localization;
using Shoootz.ViewModels.Competition;
using Shoootz.Views.Extensions;

namespace Shoootz.Views.Competition;

/// <inheritdoc/>
public partial class ShootersView : UserControl
{
    public ShootersView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnAddIconPressed(object? sender, EventArgs e)
    {
        if (DataContext is ShootersViewModel viewModel)
        {
            _ = OpenShooterDialogAsync(null, viewModel);
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ShootersViewModel viewModel)
        {
            viewModel.SaveShooterFailed += OnSaveShooterFailed;
        }
    }

    private void OnDeleteIconPressed(object? sender, EventArgs e)
    {
        if (DataContext is ShootersViewModel { SelectedShooter: { } shooter })
        {
            _ = OpenConfirmDeleteDialogAsync(shooter);
        }
    }

    private void OnEditIconPressed(object? sender, EventArgs e)
    {
        if (DataContext is ShootersViewModel { SelectedShooter: { } shooter } viewModel)
        {
            _ = OpenShooterDialogAsync(shooter, viewModel);
        }
    }

    private void OnSaveShooterFailed(string message)
    {
        _ = OpenErrorDialogAsync(message);
    }

    private void OnShooterRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (
            DataContext is ShootersViewModel viewModel
            && (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>() is { DataContext: ShooterModel shooter }
        )
        {
            _ = OpenShooterDialogAsync(shooter, viewModel);
        }
    }

    private async Task OpenConfirmDeleteDialogAsync(ShooterModel shooter)
    {
        if (DataContext is not ShootersViewModel viewModel)
        {
            return;
        }

        string fullName = $"{shooter.Firstname} {shooter.Lastname}".Trim();

        CheckBox deleteShotsCheckBox = new()
        {
            Content = LocalizationService.Instance[nameof(Messages.DeleteShooterShotsToo)],
            FontSize = 16,
            IsChecked = true,
            Margin = new Thickness(10, 12, 0, 0),
        };

        TextBlock confirmationText = LocalizationService
            .Instance[nameof(Messages.ConfirmDeleteShooterMessage)]
            .Highlight(fullName);

        StackPanel dialogContent = new() { Children = { confirmationText, deleteShotsCheckBox } };

        ContentDialog dialog = new()
        {
            DialogContent = dialogContent,
            DialogTitle = LocalizationService.Instance[nameof(Messages.DeleteShooterTitle)],
            IsDanger = true,
            SizeToContent = SizeToContent.Height,
            SubmitText = LocalizationService.Instance[nameof(Messages.Delete)],
            Width = 460,
        };

        bool? result = await DialogService.ShowAsync(this, dialog.ShowDialog<bool?>);

        if (result is true)
        {
            await viewModel.ExecuteDeleteShooterAsync(shooter, deleteShotsCheckBox.IsChecked ?? true);
        }
    }

    private async Task OpenErrorDialogAsync(string message)
    {
        await DialogService.ShowErrorDialogAsync(this, message);
    }

    private async Task OpenShooterDialogAsync(ShooterModel? shooter, ShootersViewModel viewModel)
    {
        bool isEdit = shooter is not null;

        Task<string> idTask = isEdit ? Task.FromResult(shooter!.Id!) : viewModel.GenerateShooterIdAsync();
        Task<List<string>> clubNamesTask = viewModel.GetClubNamesAsync();

        await Task.WhenAll(idTask, clubNamesTask);

        ShooterModel editingShooter = shooter ?? new ShooterModel { Id = idTask.Result };

        ShooterEditView editView = new();
        editView.SetClubOptions(clubNamesTask.Result);
        editView.LoadShooter(editingShooter);

        bool? result = await DialogService.ShowAsync(
            this,
            window =>
                new ContentDialog
                {
                    SubmitText = LocalizationService.Instance[isEdit ? nameof(Messages.Save) : nameof(Messages.Add)],
                    SubmitValidator = editView.Validate,
                    DialogContent = editView,
                    DialogTitle = string.Format(
                        LocalizationService.Instance[
                            isEdit ? nameof(Messages.EditShooterTitle) : nameof(Messages.AddShooterTitle)
                        ],
                        editingShooter.Id
                    ),
                    SizeToContent = SizeToContent.WidthAndHeight,
                }.ShowDialog<bool?>(window)
        );

        if (result is true)
        {
            await viewModel.ExecuteSaveShooterAsync(editView.BuildShooter());
        }
        else
        {
            viewModel.SelectedShooter = null;
        }
    }
}
