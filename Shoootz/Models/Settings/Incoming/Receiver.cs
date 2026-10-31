namespace Shoootz.Models.Settings.Incoming;

internal class Receiver
{
    public bool AutoConnect { get; set; }

    public string? Host { get; set; } = "192.168.0.101";

    public int Port { get; set; } = 30169;

    public ReceiverType Type { get; set; } = ReceiverType.Udp;
}
