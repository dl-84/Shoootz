using System.ComponentModel;

namespace Shoootz.Bridge.TCP.Services.Bridge;

internal interface IBridgeService : INotifyPropertyChanged
{
    int ConnectedClientCount { get; }

    bool IsRunning { get; }

    bool IsStartEnabled { get; }

    bool IsTcpPortValid { get; }

    bool IsUdpPortValid { get; }

    string? LastError { get; }

    int PacketsRelayedCount { get; }

    string TcpPort { get; set; }

    string UdpPort { get; set; }

    void Start();

    void Stop();
}
