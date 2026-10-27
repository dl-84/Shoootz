using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Controls.ContentDialog;
using Controls.TitleBar.Services;
using Shoootz.Bridge.TCP.ViewModels;
using Shoootz.Bridge.TCP.Views.Types;
using Shoootz.Common.Resources.Lang;
using Shoootz.Common.Services.Dialog;
using Shoootz.Common.Services.Localization;
using Shoootz.Common.ViewModels;

namespace Shoootz.Bridge.TCP.Views;

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

        NavBorder.Padding = new Thickness(0, PlatformInfoService.IsMac ? 36 : 0, 0, 0);

        DataContextChanged += OnDataContextChanged;
    }

    private static TextBlock BuildUnsavedChangesContent()
    {
        return new TextBlock
        {
            Text = LocalizationService.Instance[nameof(Messages.UnsavedChangesMessage)],
            TextWrapping = TextWrapping.NoWrap,
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
                    DialogTitle = LocalizationService.Instance[nameof(Messages.UnsavedChangesTitle)],
                    IsDanger = true,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    SubmitText = LocalizationService.Instance[nameof(Messages.Leave)],
                }.ShowDialog<bool?>(window)
        );

        return result is true;
    }

    private async Task HandleNavigationRequestAsync(MainWindowViewModel mainWindowViewModel, NavigationPageType page)
    {
        if (
            mainWindowViewModel.CurrentPage is IHasUnsavedChanges { IsSaveEnabled: true }
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
