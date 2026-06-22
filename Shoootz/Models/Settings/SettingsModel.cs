using System.Globalization;
using Shoootz.Models.Settings.Database;
using Shoootz.Models.Settings.Udp;

namespace Shoootz.Models.Settings;

internal class SettingsModel
{
    public string CurrentLanguageCode { get; set; } = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    public DatabaseConnection DatabaseConnection { get; init; } = new();

    public UdpConnection UdpConnection { get; init; } = new();
}
