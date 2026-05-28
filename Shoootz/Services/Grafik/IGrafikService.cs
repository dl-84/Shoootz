using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Shoootz.Services.Grafik;

internal interface IGrafikService
{
    IImage GetIcon(string name, IBrush brush);

    Bitmap GetFlag(string twoLetterIsoLanguageName);
}
