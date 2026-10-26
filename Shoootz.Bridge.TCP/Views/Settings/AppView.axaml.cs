using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Shoootz.Bridge.TCP.ViewModels.Settings;

namespace Shoootz.Bridge.TCP.Views.Settings;

/// <inheritdoc/>
public partial class AppView : UserControl
{
    public static readonly IValueConverter IsNotEmptyConverter = new FuncValueConverter<string?, bool>(value =>
        !string.IsNullOrEmpty(value)
    );

    public AppView()
    {
        InitializeComponent();
    }

    private void OnClearTcpPortIconPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is AppViewModel viewModel)
        {
            viewModel.Bridge.TcpPort = string.Empty;
        }
    }

    private void OnClearUdpPortIconPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is AppViewModel viewModel)
        {
            viewModel.Bridge.UdpPort = string.Empty;
        }
    }

    private void OnCommandIconPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is PathIcon { Tag: ICommand command })
        {
            command.Execute(null);
        }
    }
}
