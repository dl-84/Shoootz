using Avalonia.Media.Imaging;

namespace Shoootz.Services.Grafik;

internal interface IGrafikService
{
    Bitmap GetErrorTriangle { get; }

    Bitmap GetFlag(string twoLetterIsoLanguageName);
}
