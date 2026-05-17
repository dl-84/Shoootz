using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Sektionsliga.Services.Flag;

public class FlagService : IFlagService
{
    private const string BasePath = "avares://Sektionsliga/Assets/Flags/";

    private const string FileExtension = ".png";

    public Bitmap GetFlag(string twoLetterIsoLanguageName)
    {
        return new Bitmap(AssetLoader.Open(new Uri($"{BasePath}{twoLetterIsoLanguageName}{FileExtension}")));
    }
}
