using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Result;
using Result.Struct;
using Shoootz.Models.Settings.Udp;

namespace Shoootz.Services.Udp;

internal class UdpListenerService : IUdpListenerService
{
    private CancellationTokenSource? _cancellationTokenSource;

    private bool _isListening;

    private UdpClient? _client;

    public event EventHandler<bool>? IsListeningChanged;

    public event EventHandler<byte[]>? PacketReceived;

    public void Dispose() => Stop();

    public void Start(int port)
    {
        Stop();

        try
        {
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
        if (_isListening == value)
        {
            return;
        }

        _isListening = value;
        IsListeningChanged?.Invoke(this, value);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _client is not null)
        {
            try
            {
                UdpReceiveResult result = await _client.ReceiveAsync(cancellationToken);
                PacketReceived?.Invoke(this, result.Buffer);
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
