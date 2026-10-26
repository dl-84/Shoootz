using System;
using Avalonia.Controls;
using Shoootz.Bridge.TCP.ViewModels.General;
using Shoootz.Common.Services.Dialog;

namespace Shoootz.Bridge.TCP.Views.General;

/// <inheritdoc/>
public partial class GeneralView : UserControl
{
    public GeneralView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is GeneralViewModel viewModel)
        {
            viewModel.ErrorOccurred += OnErrorOccurred;
        }
    }

    private void OnErrorOccurred(string message)
    {
        _ = DialogService.ShowErrorDialogAsync(this, message);
    }
}
