using System.Globalization;
using Avalonia.Media.Imaging;

namespace Shoootz.Models;

internal class LanguageOptionModel(string twoLetterIsoLanguageName, Bitmap flag)
{
    public CultureInfo CultureInfo { get; } = new(twoLetterIsoLanguageName);

    public Bitmap Flag { get; } = flag;
}
