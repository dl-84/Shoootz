using System.Globalization;

namespace Sektionsliga.Models;

public class AppSettingsModel
{
    public string CurrentLanguageCode { get; init; } = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
}
