using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Controls.ContentDialog;
using Controls.InfoDialog;
using Controls.InfoDialog.Enum;
using Controls.WaitingDialog;
using Shoootz.Common.Resources.Lang;
using Shoootz.Common.Services.Localization;

namespace Shoootz.Common.Services.Dialog;

public static class DialogService
{
    public static async Task<string?> RunWithWaitingDialogAsync(
        UserControl view,
        string waitingTitle,
        string waitingMessage,
        Func<Task<string?>> operation
    )
    {
        if (TopLevel.GetTopLevel(view) is not Window window)
        {
            return null;
        }

        return await RunWithWaitingDialogAsync(window, waitingTitle, waitingMessage, operation);
    }

    public static async Task<string?> RunWithWaitingDialogAsync(
        Window window,
        string waitingTitle,
        string waitingMessage,
        Func<Task<string?>> operation
    )
    {
        WaitingDialog waitingDialog = new() { DialogTitle = waitingTitle, Message = waitingMessage };

        _ = waitingDialog.ShowDialog(window);

        string? errorMessage = await operation();

        waitingDialog.Close();

        if (errorMessage is not null)
        {
            await ShowErrorDialogAsync(window, errorMessage);
        }

        return errorMessage;
    }

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
        IDialogHost? dialogHost = window.DataContext as IDialogHost;

        try
        {
            dialogHost?.IsDialogOpen = true;

            return await showDialog(window);
        }
        finally
        {
            dialogHost?.IsDialogOpen = false;
        }
    }

    public static async Task<bool> ShowConfirmDialogAsync(
        UserControl view,
        string title,
        string message,
        string submitText
    )
    {
        if (TopLevel.GetTopLevel(view) is not Window window)
        {
            return false;
        }

        return await ShowConfirmDialogAsync(window, title, message, submitText);
    }

    public static async Task<bool> ShowConfirmDialogAsync(
        Window window,
        string title,
        string message,
        string submitText
    )
    {
        bool? result = await ShowAsync(
            window,
            dialogWindow =>
                new ContentDialog
                {
                    DialogContent = new TextBlock
                    {
                        MaxWidth = 360,
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                    },
                    DialogTitle = title,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    SubmitText = submitText,
                }.ShowDialog<bool?>(dialogWindow)
        );

        return result == true;
    }

    public static async Task<bool> ShowDbStatusDialogAsync(UserControl view, bool isUpdate)
    {
        if (TopLevel.GetTopLevel(view) is not Window window)
        {
            return false;
        }

        return await ShowDbStatusDialogAsync(window, isUpdate);
    }

    public static async Task<bool> ShowDbStatusDialogAsync(Window window, bool isUpdate)
    {
        string title = LocalizationService.Instance[
            isUpdate ? Messages.DbPendingMigrationsTitle : Messages.DbNotInitializedDialogTitle
        ];

        string message = LocalizationService.Instance[
            isUpdate ? Messages.DbPendingMigrations : Messages.DbNotInitializedDialogMessage
        ];

        string submitText = LocalizationService.Instance[isUpdate ? Messages.ApplyDbUpdate : Messages.InitializeDb];

        return await ShowConfirmDialogAsync(window, title, message, submitText);
    }

    public static async Task ShowErrorDialogAsync(UserControl view, string message)
    {
        if (TopLevel.GetTopLevel(view) is not Window window)
        {
            return;
        }

        await ShowErrorDialogAsync(window, message);
    }

    public static async Task ShowErrorDialogAsync(Window window, string message)
    {
        await ShowAsync(
            window,
            dialogWindow =>
                new InfoDialog
                {
                    DialogTitle = LocalizationService.Instance[nameof(Messages.Error)],
                    IconType = IconType.Error,
                    Message = message,
                }.ShowDialog(dialogWindow)
        );
    }
}
