using Avalonia.Media.Imaging;

namespace Shoootz.Common.Services.Graphics;

public interface IGraphicsService
{
    Bitmap GetFlag(string twoLetterIsoLanguageName);
}
