using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Result;
using Result.Struct;
using Shoootz.Models.Settings.Udp;

namespace Shoootz.Services.Udp;

internal class UdpListenerService : IUdpListenerService
{
    private readonly Channel<byte[]> _channel = Channel.CreateUnbounded<byte[]>();

    private CancellationTokenSource? _cancellationTokenSource;

    private UdpClient? _client;

    private string? _ipAddressFilter;

    public event EventHandler<bool>? IsListeningChanged;

    public bool IsListening { get; private set; }

    public ChannelReader<byte[]> Reader => _channel.Reader;

    public void Dispose() => Stop();

    public void Start(string ipAddress, int port)
    {
        Stop();

        try
        {
            _ipAddressFilter = ipAddress;
            _client = new UdpClient(port);
            _cancellationTokenSource = new CancellationTokenSource();

            SetIsListening(true);
            _ = ReceiveLoopAsync(_cancellationTokenSource.Token);
        }
        catch
        {
            SetIsListening(false);
        }
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _client?.Dispose();
        _client = null;
        SetIsListening(false);
    }

    public Result<bool, UdpError> TestConnection(int port)
    {
        try
        {
            using UdpClient client = new UdpClient(port);
            return true;
        }
        catch (Exception exception)
        {
            return new Error<UdpError>(new UdpError(exception.Message));
        }
    }

    private void SetIsListening(bool value)
    {
        if (IsListening == value)
        {
            return;
        }

        IsListening = value;
        IsListeningChanged?.Invoke(this, value);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _client is not null)
        {
            try
            {
                UdpReceiveResult result = await _client.ReceiveAsync(cancellationToken);

                if (string.Equals(result.RemoteEndPoint.Address.ToString(), _ipAddressFilter))
                {
                    _channel.Writer.TryWrite(result.Buffer);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                break;
            }
        }

        SetIsListening(false);
    }
}
