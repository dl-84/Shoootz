using System.Globalization;
using Avalonia.Media.Imaging;

namespace Sektionsliga.Models;

public class LanguageOptionModel(string twoLetterIsoLanguageName, Bitmap flag)
{
    public CultureInfo CultureInfo { get; } = new(twoLetterIsoLanguageName);

    public Bitmap Flag { get; } = flag;
}
