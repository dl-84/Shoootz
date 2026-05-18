using Avalonia.Media.Imaging;

namespace Sektionsliga.Services.Grafik;

internal interface IGrafikService
{
    Bitmap GetErrorTriangle { get; }

    Bitmap GetFlag(string twoLetterIsoLanguageName);
}
