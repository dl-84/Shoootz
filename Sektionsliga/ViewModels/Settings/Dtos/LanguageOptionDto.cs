using System.Globalization;
using Avalonia.Media.Imaging;

namespace Sektionsliga.ViewModels.Settings.Dtos;

public class LanguageOptionDto(string twoLetterIsoLanguageName, Bitmap flag)
{
    public CultureInfo CultureInfo { get; } = new(twoLetterIsoLanguageName);
    public Bitmap Flag { get; } = flag;
}
