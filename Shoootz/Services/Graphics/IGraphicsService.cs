using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Shoootz.Services.Graphics;

internal interface IGraphicsService
{
    IImage GetIcon(string name, IBrush brush);

    Bitmap GetFlag(string twoLetterIsoLanguageName);
}
