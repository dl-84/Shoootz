namespace Shoootz.Common.Services.Settings;

public static class PortValidator
{
    public static bool IsValid(string value) => int.TryParse(value, out int port) && port is >= 1 and <= 65535;
}
