using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Controls.InfoDialog;
using Shoootz.Services.Localization;
using Shoootz.ViewModels;
using Shoootz.ViewModels.Settings;

namespace Shoootz.Views.Settings;

/// <inheritdoc />
public partial class DatabaseView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseView"/> class.
    /// </summary>
    public DatabaseView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is DatabaseViewModel viewModel)
        {
            viewModel.ConnectionTestCompleted += OnConnectionTestCompleted;
        }
    }

    private void OnConnectionTestCompleted(bool success, string? errorMessage)
    {
        _ = OpenConnectionTestDialogAsync(success, errorMessage);
    }

    private async Task OpenConnectionTestDialogAsync(bool success, string? errorMessage)
    {
        if (TopLevel.GetTopLevel(this) is not Window window)
        {
            return;
        }

        MainWindowViewModel? mainWindowViewModel = window.DataContext as MainWindowViewModel;

        try
        {
            if (mainWindowViewModel is not null)
            {
                mainWindowViewModel.IsDialogOpen = true;
            }

            await new InfoDialog
            {
                CloseText = LocalizationService.Instance["Close"],
                DialogTitle = LocalizationService.Instance["TestConnection"],
                IconType = success ? IconType.Info : IconType.Warning,
                Message = success
                    ? LocalizationService.Instance["ConnectionSuccessful"]
                    : errorMessage ?? LocalizationService.Instance["ConnectionFailed"],
            }.ShowDialog(window);
        }
        finally
        {
            if (mainWindowViewModel is not null)
            {
                mainWindowViewModel.IsDialogOpen = false;
            }
        }
    }
}
