using System.Text.Json.Serialization;

namespace Shoootz.Models.Settings.Incoming;

internal class Receiver
{
    public const string DefaultHost = "192.168.0.101";

    public bool AutoConnect { get; set; }

    public string? Host { get; set; } = DefaultHost;

    public int Port { get; set; } = 30169;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ReceiverType Type { get; set; } = ReceiverType.Udp;
}
