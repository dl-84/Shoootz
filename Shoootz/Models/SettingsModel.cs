using System.Globalization;

namespace Shoootz.Models;

internal class SettingsModel
{
    public string CurrentLanguageCode { get; set; } = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
}
