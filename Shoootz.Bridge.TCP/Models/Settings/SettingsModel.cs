using System.Globalization;
using Shoootz.Common.Models.Language;

namespace Shoootz.Bridge.TCP.Models.Settings;

internal class SettingsModel : ILanguageSettings
{
    public bool AutoConnect { get; set; } = false;

    public string CurrentLanguageCode { get; set; } = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    public int TcpPort { get; set; } = 30170;

    public int UdpPort { get; set; } = 30169;
}
