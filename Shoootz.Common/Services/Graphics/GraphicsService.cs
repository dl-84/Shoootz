using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Shoootz.Common.Services.Graphics;

public class GraphicsService : IGraphicsService
{
    private const string BasePathFlags = "avares://Shoootz.Common/Assets/Flags/";

    public Bitmap GetFlag(string twoLetterIsoLanguageName)
    {
        return new Bitmap(AssetLoader.Open(new Uri($"{BasePathFlags}{twoLetterIsoLanguageName}.png")));
    }
}
