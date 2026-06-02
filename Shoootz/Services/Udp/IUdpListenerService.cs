using System;
using Result;
using Shoootz.Models.Error;

namespace Shoootz.Services.Udp;

internal interface IUdpListenerService : IDisposable
{
    event EventHandler<bool> IsListeningChanged;

    bool IsListening { get; }

    void Start(string ipAddress, int port);

    void Stop();

    Result<bool, UdpConnectionError> TestConnection(int port);
}
