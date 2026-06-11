namespace Shoootz.Models.Settings;

internal enum SettingsPropertyType
{
    CurrentLanguageCode,
    DatabaseConnectionString,
    DatabaseProvider,
    ExceptionOnReadContent,
    JsonExceptionOnValidate,
    UdpAutoConnect,
    UdpPort,
    Unknown,
}

internal static class StringExtensions
{
    extension(string? value)
    {
        public SettingsPropertyType? ToSettingsProperty() =>
            value switch
            {
                "AutoConnect" => SettingsPropertyType.UdpAutoConnect,
                "ConnectionString" => SettingsPropertyType.DatabaseConnectionString,
                "CurrentLanguageCode" => SettingsPropertyType.CurrentLanguageCode,
                "Port" => SettingsPropertyType.UdpPort,
                "Provider" => SettingsPropertyType.DatabaseProvider,
                _ => SettingsPropertyType.Unknown,
            };
    }
}
