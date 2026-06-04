using System;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg.Skia;

namespace Shoootz.Services.Graphics;

internal class GraphicsService : IGraphicsService
{
    private const string BasePathFlags = "avares://Shoootz/Assets/Flags/";

    private const string BasePathIcons = "avares://Shoootz/Assets/Icons/";

    public IImage GetIcon(string name, IBrush brush)
    {
        return new SvgImage { Source = SvgSource.Load($"{BasePathIcons}{name}.svg"), Css = BuildCss(brush) };
    }

    public Bitmap GetFlag(string twoLetterIsoLanguageName)
    {
        return new Bitmap(AssetLoader.Open(new Uri($"{BasePathFlags}{twoLetterIsoLanguageName}.png")));
    }

    private static string BuildCss(IBrush brush)
    {
        if (brush is SolidColorBrush solid)
        {
            Color color = solid.Color;
            return $"* {{ fill: #{color.R:X2}{color.G:X2}{color.B:X2}; }}";
        }

        return string.Empty;
    }
}
