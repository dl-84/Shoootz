using System.Net;
using System.Net.Sockets;

namespace Shoootz.Common.Extensions;

public static class IPv4Extensions
{
    public static bool IsValidIPv4(this string value) =>
        IPAddress.TryParse(value, out IPAddress? address) && address.AddressFamily == AddressFamily.InterNetwork;
}
