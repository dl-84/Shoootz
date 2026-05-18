using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Sektionsliga.Services.Grafik;

internal class GrafikService : IGrafikService
{
    private const string BasePathFlags = "avares://Sektionsliga/Assets/Flags/";

    private const string BasePathIcons = "avares://Sektionsliga/Assets/Icons/";

    private const string FileExtension = ".png";

    public Bitmap GetErrorTriangle =>
        new(AssetLoader.Open(new Uri($"{BasePathIcons}TriangleExclamationRed{FileExtension}")));

    public Bitmap GetFlag(string twoLetterIsoLanguageName)
    {
        return new Bitmap(AssetLoader.Open(new Uri($"{BasePathFlags}{twoLetterIsoLanguageName}{FileExtension}")));
    }
}
