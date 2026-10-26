using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Controls.ContentDialog;
using Shoootz.Common.Resources.Lang;
using Shoootz.Common.Services.Dialog;
using Shoootz.Common.Services.Localization;
using Shoootz.ViewModels.Competition;

namespace Shoootz.Views.Competition;

/// <inheritdoc/>
public partial class ShotsView : UserControl
{
    public ShotsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        ShotsListBox.AddHandler(InputElement.PointerPressedEvent, OnRowPointerPressed, RoutingStrategies.Tunnel);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ShotsViewModel viewModel)
        {
            viewModel.DeleteShotsFailed += OnDeleteShotsFailed;
            viewModel.LoadShotsFailed += OnDeleteShotsFailed;
        }
    }

    private void OnDeleteIconPressed(object? sender, EventArgs e)
    {
        if (DataContext is ShotsViewModel viewModel)
        {
            _ = OpenConfirmDeleteDialogAsync(viewModel);
        }
    }

    private void OnDeleteShotsFailed(string message)
    {
        _ = DialogService.ShowErrorDialogAsync(this, message);
    }

    private void OnRefreshIconPressed(object? sender, EventArgs e)
    {
        if (DataContext is ShotsViewModel viewModel)
        {
            _ = viewModel.ExecuteRefreshAsync();
        }
    }

    private void OnRowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.ClickCount != 1 || e.Source is CheckBox)
        {
            return;
        }

        if ((e.Source as Visual)?.FindAncestorOfType<ListBoxItem>() is not { DataContext: ShotRowViewModel row })
        {
            return;
        }

        row.IsSelected = !row.IsSelected;
        e.Handled = true;
    }

    private async Task OpenConfirmDeleteDialogAsync(ShotsViewModel viewModel)
    {
        if (!viewModel.Shots.Any(row => row.IsSelected))
        {
            return;
        }

        ContentDialog dialog = new()
        {
            DialogContent = LocalizationService.Instance[nameof(Messages.ConfirmDeleteShotsMessage)],
            DialogTitle = LocalizationService.Instance[nameof(Messages.DeleteShotsTitle)],
            IsDanger = true,
            SizeToContent = SizeToContent.Height,
            SubmitText = LocalizationService.Instance[nameof(Messages.Delete)],
            Width = 460,
        };

        bool? result = await DialogService.ShowAsync(this, window => dialog.ShowDialog<bool?>(window));

        if (result is true)
        {
            await viewModel.ExecuteDeleteSelectedShotsAsync();
        }
    }
}
