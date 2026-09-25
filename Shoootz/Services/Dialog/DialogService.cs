using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Controls.InfoDialog;
using Controls.InfoDialog.Enum;
using Shoootz.Resources.Lang;
using Shoootz.Services.Localization;
using Shoootz.ViewModels;

namespace Shoootz.Services.Dialog;

internal static class DialogService
{
    public static async Task ShowAsync(UserControl view, Func<Window, Task> showDialog)
    {
        if (TopLevel.GetTopLevel(view) is not Window window)
        {
            return;
        }

        await ShowAsync(window, showDialog);
    }

    public static async Task ShowAsync(Window window, Func<Window, Task> showDialog)
    {
        await ShowAsync<object?>(
            window,
            async w =>
            {
                await showDialog(w);
                return null;
            }
        );
    }

    public static async Task<TResult?> ShowAsync<TResult>(UserControl view, Func<Window, Task<TResult>> showDialog)
    {
        if (TopLevel.GetTopLevel(view) is not Window window)
        {
            return default;
        }

        return await ShowAsync(window, showDialog);
    }

    public static async Task<TResult?> ShowAsync<TResult>(Window window, Func<Window, Task<TResult>> showDialog)
    {
        MainWindowViewModel? mainWindowViewModel = window.DataContext as MainWindowViewModel;

        try
        {
            mainWindowViewModel?.IsDialogOpen = true;
            return await showDialog(window);
        }
        finally
        {
            mainWindowViewModel?.IsDialogOpen = false;
        }
    }

    public static async Task ShowErrorDialogAsync(UserControl view, string message)
    {
        await ShowAsync(
            view,
            window =>
                new InfoDialog
                {
                    DialogTitle = LocalizationService.Instance[nameof(Messages.Error)],
                    IconType = IconType.Error,
                    Message = message,
                }.ShowDialog(window)
        );
    }
}
