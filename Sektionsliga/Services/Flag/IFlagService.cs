using Avalonia.Media.Imaging;

namespace Sektionsliga.Services.Flag;

public interface IFlagService
{
    Bitmap GetFlag(string twoLetterIsoLanguageName);
}
