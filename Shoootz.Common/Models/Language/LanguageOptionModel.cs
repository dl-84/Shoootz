using System.Globalization;
using Avalonia.Media.Imaging;

namespace Shoootz.Common.Models.Language;

public class LanguageOptionModel(string twoLetterIsoLanguageName, Bitmap flag)
{
    public CultureInfo CultureInfo { get; } = new(twoLetterIsoLanguageName);

    public Bitmap Flag { get; } = flag;
}
