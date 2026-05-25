using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Controls.InfoDialog;
using Shoootz.Services.App;
using Shoootz.Services.Localization;
using Shoootz.ViewModels;
using Shoootz.ViewModels.Settings;

namespace Shoootz.Views.Settings;

/// <inheritdoc />
public partial class DatabaseView : UserControl
{
    private static readonly Geometry InfoIconData = Geometry.Parse(
        "M320 576C461.4 576 576 461.4 576 320C576 178.6 461.4 64 320 64C178.6 64 64 178.6 64 320C64 461.4 178.6 576 320 576z"
            + "M288 224C288 206.3 302.3 192 320 192C337.7 192 352 206.3 352 224C352 241.7 337.7 256 320 256C302.3 256 288 241.7 288 224z"
            + "M280 288L328 288C341.3 288 352 298.7 352 312L352 400L360 400C373.3 400 384 410.7 384 424C384 437.3 373.3 448 360 448"
            + "L280 448C266.7 448 256 437.3 256 424C256 410.7 266.7 400 280 400L304 400L304 336L280 336C266.7 336 256 325.3 256 312"
            + "C256 298.7 266.7 288 280 288z"
    );

    private static readonly Geometry TriangleIconData = Geometry.Parse(
        "M320 64C334.7 64 348.2 72.1 355.2 85L571.2 485C577.9 497.4 577.6 512.4 570.4 524.5C563.2 536.6 550.1 544 536 544"
            + "L104 544C89.9 544 76.8 536.6 69.6 524.5C62.4 512.4 62.1 497.4 68.8 485L284.8 85C291.8 72.1 305.3 64 320 64z"
            + "M320 416C302.3 416 288 430.3 288 448C288 465.7 302.3 480 320 480C337.7 480 352 465.7 352 448C352 430.3 337.7 416 320 416z"
            + "M320 224C301.8 224 287.3 239.5 288.6 257.7L296 361.7C296.9 374.2 307.4 384 319.9 384C332.5 384 342.9 374.3 343.8 361.7"
            + "L351.2 257.7C352.5 239.5 338.1 224 319.8 224z"
    );

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
                BackgroundColor = AppBrush.Background,
                CloseButtonColor = AppBrush.Green,
                CloseText = LocalizationService.Instance["Close"],
                DialogTitle = LocalizationService.Instance["TestConnection"],
                IconBrush = success ? AppBrush.Green : AppBrush.Error,
                IconData = success ? InfoIconData : TriangleIconData,
                Message = success
                    ? LocalizationService.Instance["ConnectionSuccessful"]
                    : errorMessage ?? LocalizationService.Instance["ConnectionFailed"],
                PrimaryColor = AppBrush.Primary,
                TextColor = AppBrush.PrimaryForeground,
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
