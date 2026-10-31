using System.Globalization;
using Shoootz.Common.Models.Language;
using Shoootz.Models.Settings.Database;
using Shoootz.Models.Settings.Incoming;

namespace Shoootz.Models.Settings;

internal class SettingsModel : ILanguageSettings
{
    public string CurrentLanguageCode { get; set; } = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    public DatabaseConnection DatabaseConnection { get; init; } = new();

    public Receiver Receiver { get; init; } = new();
}
