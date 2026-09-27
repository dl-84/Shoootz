using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
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

    private void OnAddIconPressed(object? sender, PointerPressedEventArgs e)
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

    private void OnDeleteIconPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is ShootersViewModel { SelectedShooter: { } shooter })
        {
            _ = OpenConfirmDeleteDialogAsync(shooter);
        }
    }

    private void OnEditIconPressed(object? sender, PointerPressedEventArgs e)
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
            DialogTitle = LocalizationService.Instance[nameof(Messages.Delete)],
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
                    DialogTitle = LocalizationService.Instance[nameof(Messages.Shooters)],
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
