using System.Globalization;
using Avalonia.Media.Imaging;
using Sektionsliga.Services.Flag;

namespace Sektionsliga.ViewModels.Settings.Dtos;

public class LanguageOptionDto(string twoLetterIsoLanguageName)
{
    public CultureInfo CultureInfo { get; private set; } = new(twoLetterIsoLanguageName);

    public Bitmap Flag { get; private set; } = FlagService.GetFromTwoLetterIsoLanguageName(twoLetterIsoLanguageName);
}
