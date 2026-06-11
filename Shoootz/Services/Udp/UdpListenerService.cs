using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Result;
using Result.Types;
using Shoootz.Models.Error;

namespace Shoootz.Services.Udp;

internal class UdpListenerService : IUdpListenerService
{
    private CancellationTokenSource? _cancellationTokenSource;

    private UdpClient? _client;

    public event EventHandler<bool>? IsListeningChanged;

    public event EventHandler<byte[]>? ShotRawDataReceived;

    public bool IsListening { get; private set; }

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

    public Result<bool, UdpConnectionError> TestConnection(int port)
    {
        try
        {
            using UdpClient client = new UdpClient(port);
            return true;
        }
        catch (Exception exception)
        {
            return new Error<UdpConnectionError>(new UdpConnectionError(exception.Message));
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

                ShotRawDataReceived?.Invoke(this, result.Buffer);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // continue loop on transient errors (e.g. null RemoteEndPoint)
            }
        }

        SetIsListening(false);
    }
}
