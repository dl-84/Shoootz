using System.Globalization;

namespace Sektionsliga.Models;

public class AppSettingsModel
{
    public string LanguageCultureCode { get; init; } = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
}
