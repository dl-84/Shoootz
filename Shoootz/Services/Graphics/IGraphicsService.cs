using Avalonia.Media.Imaging;

namespace Shoootz.Services.Graphics;

internal interface IGraphicsService
{
    Bitmap GetFlag(string twoLetterIsoLanguageName);
}
