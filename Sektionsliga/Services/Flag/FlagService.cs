using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Sektionsliga.Services.Flag;

public static class FlagService
{
    private const string BasePath = "avares://Sektionsliga/Assets/Flags/";

    private const string FileExtension = ".png";

    public static Bitmap GetFromTwoLetterIsoLanguageName(string twoLetterIsoLanguageName)
    {
        return new Bitmap(AssetLoader.Open(new Uri($"{BasePath}{twoLetterIsoLanguageName}{FileExtension}")));
    }
}
