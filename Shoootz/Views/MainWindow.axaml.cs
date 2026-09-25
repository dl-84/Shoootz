using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Controls.ContentDialog;
using Controls.InfoDialog;
using Controls.InfoDialog.Enum;
using Shoootz.Resources.Lang;
using Shoootz.Services.Dialog;
using Shoootz.Services.Localization;
using Shoootz.ViewModels;
using Shoootz.ViewModels.Settings;

namespace Shoootz.Views;

/// <inheritdoc/>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            RootBorder.CornerRadius = new CornerRadius(0);
        }

        DataContextChanged += OnDataContextChanged;
    }

    protected override void OnOpened(EventArgs eventArgs)
    {
        base.OnOpened(eventArgs);

        if (DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        mainWindowViewModel.PendingMigrationsDetected += OnPendingMigrationsDetected;
        _ = mainWindowViewModel.CheckPendingMigrationsAsync();
    }

    private static TextBlock BuildUnsavedChangesContent()
    {
        return new TextBlock
        {
            Text = LocalizationService.Instance[Messages.UnsavedChangesMessage],
            TextWrapping = Avalonia.Media.TextWrapping.NoWrap,
        };
    }

    private async Task<bool> ConfirmLeaveUnsavedChangesAsync()
    {
        bool? result = await DialogService.ShowAsync(
            this,
            window =>
                new ContentDialog
                {
                    DialogContent = BuildUnsavedChangesContent(),
                    DialogTitle = LocalizationService.Instance[Messages.UnsavedChangesTitle],
                    IsDanger = true,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    SubmitText = LocalizationService.Instance[Messages.Leave],
                }.ShowDialog<bool?>(window)
        );

        return result is true;
    }

    private async Task HandleNavigationRequestAsync(MainWindowViewModel mainWindowViewModel, NavigationPageType page)
    {
        if (
            mainWindowViewModel.CurrentPage is ConnectionViewModel { IsSaveEnabled: true }
            && !await ConfirmLeaveUnsavedChangesAsync()
        )
        {
            SyncSelectedNavItem(mainWindowViewModel.CurrentNavigationPageType);
            return;
        }

        mainWindowViewModel.NavigateTo(page);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        mainWindowViewModel.PropertyChanged += OnViewModelPropertyChanged;
        SyncSelectedNavItem(mainWindowViewModel.CurrentNavigationPageType);
    }

    private void OnNavSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (
            e.AddedItems.Count > 0
            && e.AddedItems[0] is ListBoxItem { Tag: NavigationPageType page }
            && DataContext is MainWindowViewModel mainWindowViewModel
            && page != mainWindowViewModel.CurrentNavigationPageType
        )
        {
            _ = HandleNavigationRequestAsync(mainWindowViewModel, page);
        }
    }

    private void OnPendingMigrationsDetected()
    {
        _ = OpenPendingMigrationsDialogAsync();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (
            e.PropertyName == nameof(MainWindowViewModel.CurrentNavigationPageType)
            && DataContext is MainWindowViewModel mainWindowViewModel
        )
        {
            SyncSelectedNavItem(mainWindowViewModel.CurrentNavigationPageType);
        }
    }

    private async Task OpenPendingMigrationsDialogAsync()
    {
        await DialogService.ShowAsync(
            this,
            window =>
                new InfoDialog
                {
                    DialogTitle = LocalizationService.Instance[Messages.DbPendingMigrationsTitle],
                    IconType = IconType.Warning,
                    Message = LocalizationService.Instance[Messages.DbPendingMigrations],
                }.ShowDialog(window)
        );
    }

    private void SyncSelectedNavItem(NavigationPageType pageType)
    {
        ListBoxItem? match = NavList
            .GetLogicalDescendants()
            .OfType<ListBoxItem>()
            .FirstOrDefault(item => item.Tag is NavigationPageType tag && tag == pageType);

        if (match is not null)
        {
            NavList.SelectedItem = match;
        }
    }
}
