using System;
using Result;
using Shoootz.Models.Error;

namespace Shoootz.Services.Udp;

internal interface IUdpListenerService : IDisposable
{
    event EventHandler<bool> IsListeningChanged;

    event EventHandler<byte[]> ShotRawDataReceived;

    bool IsListening { get; }

    void Start(string ipAddress, int port);

    void Stop();

    Result<bool, UdpConnectionError> TestConnection(int port);
}
