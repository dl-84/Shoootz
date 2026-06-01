using System;
using Result;
using Shoootz.Models.Settings.Udp;

namespace Shoootz.Services.Udp;

internal interface IUdpListenerService : IDisposable
{
    event EventHandler<bool> IsListeningChanged;

    event EventHandler<byte[]> PacketReceived;

    void Start(int port);

    void Stop();

    Result<bool, UdpError> TestConnection(int port);
}
