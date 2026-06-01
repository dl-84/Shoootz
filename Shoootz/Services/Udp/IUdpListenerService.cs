using System;
using System.Threading.Channels;
using Result;
using Shoootz.Models.Settings.Udp;

namespace Shoootz.Services.Udp;

internal interface IUdpListenerService : IDisposable
{
    event EventHandler<bool> IsListeningChanged;

    bool IsListening { get; }

    ChannelReader<byte[]> Reader { get; }

    void Start(string ipAddress, int port);

    void Stop();

    Result<bool, UdpError> TestConnection(int port);
}
