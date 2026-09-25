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
    public static async Task ShowErrorDialogAsync(UserControl view, string message)
    {
        if (TopLevel.GetTopLevel(view) is not Window window)
        {
            return;
        }

        MainWindowViewModel? mainWindowViewModel = window.DataContext as MainWindowViewModel;

        try
        {
            mainWindowViewModel?.IsDialogOpen = true;

            await new InfoDialog
            {
                DialogTitle = LocalizationService.Instance[nameof(Messages.Error)],
                IconType = IconType.Error,
                Message = message,
            }.ShowDialog(window);
        }
        finally
        {
            mainWindowViewModel?.IsDialogOpen = false;
        }
    }
}
