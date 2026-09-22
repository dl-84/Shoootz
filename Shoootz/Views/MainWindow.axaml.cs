using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Controls.InfoDialog;
using Controls.InfoDialog.Enum;
using Shoootz.Resources.Lang;
using Shoootz.Services.Localization;
using Shoootz.ViewModels;

namespace Shoootz.Views;

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
            mainWindowViewModel.NavigateTo(page);
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
        if (DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        try
        {
            mainWindowViewModel.IsDialogOpen = true;

            await new InfoDialog
            {
                CloseText = LocalizationService.Instance[Messages.Close],
                DialogTitle = LocalizationService.Instance[Messages.DbPendingMigrationsTitle],
                IconType = IconType.Warning,
                Message = LocalizationService.Instance[Messages.DbPendingMigrations],
            }.ShowDialog(this);
        }
        finally
        {
            mainWindowViewModel.IsDialogOpen = false;
        }
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
