namespace Shoootz.Models.Settings.Udp;

internal class UdpConnectionModel
{
    public bool AutoConnect { get; set; } = false;

    public string IpAddress { get; set; } = "127.0.0.1";

    public int Port { get; set; } = 30169;
}
