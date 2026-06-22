namespace Shoootz.Models.Settings.Udp;

internal class UdpConnection
{
    public bool AutoConnect { get; set; } = false;

    public int Port { get; set; } = 30169;
}
