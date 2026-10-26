namespace Shoootz.Common.Extensions;

public static class PortExtensions
{
    public static bool IsValidPort(this string value) => int.TryParse(value, out int port) && port is >= 1 and <= 65535;
}
