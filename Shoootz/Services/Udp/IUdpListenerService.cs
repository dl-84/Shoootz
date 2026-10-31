using System;
using Result;
using Result.Types;
using Shoootz.Models.Error;

namespace Shoootz.Services.Udp;

internal interface IUdpListenerService : IDisposable
{
    event EventHandler<bool> IsListeningChanged;

    event EventHandler<byte[]> ShotRawDataReceived;

    bool IsListening { get; }

    Result<Unit, UdpConnectionErrorModel> Start(int port);

    void Stop();

    Result<bool, UdpConnectionErrorModel> TestConnection(int port);
}
