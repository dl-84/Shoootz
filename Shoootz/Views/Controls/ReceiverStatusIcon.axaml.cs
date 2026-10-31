using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Svg.Skia;

namespace Shoootz.Views.Controls;

internal partial class ReceiverStatusIcon : UserControl
{
    public static readonly StyledProperty<bool> IsConnectedProperty = AvaloniaProperty.Register<
        ReceiverStatusIcon,
        bool
    >(nameof(IsConnected));

    public static readonly StyledProperty<bool> IsTcpProperty = AvaloniaProperty.Register<ReceiverStatusIcon, bool>(
        nameof(IsTcp)
    );

    public ReceiverStatusIcon()
    {
        InitializeComponent();
    }

    public bool IsConnected
    {
        get => GetValue(IsConnectedProperty);
        set => SetValue(IsConnectedProperty, value);
    }

    public bool IsTcp
    {
        get => GetValue(IsTcpProperty);
        set => SetValue(IsTcpProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateImage();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsConnectedProperty || change.Property == IsTcpProperty)
        {
            UpdateImage();
        }
    }

    private static string BuildCss(IBrush? brush)
    {
        if (brush is SolidColorBrush solid)
        {
            Color color = solid.Color;
            return $"* {{ fill: #{color.R:X2}{color.G:X2}{color.B:X2}; }}";
        }

        return string.Empty;
    }

    private static IBrush? GetThemeBrush(string key)
    {
        if (Application.Current is null)
        {
            return null;
        }

        Application.Current.TryGetResource(key, Application.Current.ActualThemeVariant, out object? resource);
        return resource as IBrush;
    }

    private void UpdateImage()
    {
        string name = IsTcp ? "TcpIcon" : "UdpIcon";
        IBrush? brush = GetThemeBrush(IsConnected ? "SuccessBrush" : "ErrorBrush");

        IconImage.Source = new SvgImage
        {
            Source = SvgSource.Load($"avares://Icons/Assets/{name}.svg", null),
            Css = BuildCss(brush),
        };
    }
}
