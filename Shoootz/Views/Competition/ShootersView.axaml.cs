using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Controls.ContentDialog;
using Shoootz.Models.Shooter;
using Shoootz.Resources.Lang;
using Shoootz.Services.Dialog;
using Shoootz.Services.Localization;
using Shoootz.ViewModels.Competition;

namespace Shoootz.Views.Competition;

/// <inheritdoc/>
public partial class ShootersView : UserControl
{
    public ShootersView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        ShootersListBox.AddHandler(PointerPressedEvent, OnShooterRowPointerPressed, RoutingStrategies.Tunnel);
    }

    private static TextBlock BuildShooterDeleteContent(string shooterName)
    {
        string template = LocalizationService.Instance[nameof(Messages.ConfirmDeleteShooterMessage)];
        string[] parts = template.Split("{0}", 2);

        TextBlock textBlock = new() { FontSize = 16, TextWrapping = TextWrapping.Wrap };

        if (parts.Length == 2)
        {
            textBlock.Inlines =
            [
                new Run(parts[0]),
                new Bold
                {
                    FontWeight = FontWeight.Bold,
                    Inlines = [new Run(shooterName) { FontSize = 16, FontWeight = FontWeight.Bold }],
                },
                new Run(parts[1]),
            ];
        }
        else
        {
            textBlock.Inlines = [new Run(template)];
        }

        return textBlock;
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

    private void OnShooterRowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (
            e.ClickCount == 1
            && DataContext is ShootersViewModel viewModel
            && (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>() is { DataContext: ShooterModel shooter }
            && ReferenceEquals(shooter, viewModel.SelectedShooter)
        )
        {
            viewModel.SelectedShooter = null;
            e.Handled = true;
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

        StackPanel dialogContent = new() { Children = { BuildShooterDeleteContent(fullName), deleteShotsCheckBox } };

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
        ShooterModel editingShooter = shooter ?? new ShooterModel { Id = await viewModel.GenerateShooterIdAsync() };

        ShooterEditView editView = new();
        editView.SetClubOptions(await viewModel.GetClubNamesAsync());
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
