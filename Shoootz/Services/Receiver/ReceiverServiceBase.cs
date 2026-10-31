using System;
using System.Threading;
using Result;
using Result.Types;
using Shoootz.Models.Error;

namespace Shoootz.Services.Receiver;

internal abstract class ReceiverServiceBase : IReceiverService
{
    private CancellationTokenSource? _cancellationTokenSource;

    public event EventHandler<bool>? IsListeningChanged;

    public event EventHandler<byte[]>? ShotRawDataReceived;

    public bool IsListening { get; private set; }

    public abstract Result<Unit, ReceiverConnectionErrorModel> Connect();

    public virtual void Disconnect()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        SetIsListening(false);
    }

    public void Dispose() => Disconnect();

    public abstract Result<bool, ReceiverConnectionErrorModel> TestConnection();

    protected void RaiseShotRawDataReceived(byte[] data) => ShotRawDataReceived?.Invoke(this, data);

    protected void SetIsListening(bool value)
    {
        if (IsListening == value)
        {
            return;
        }

        IsListening = value;
        IsListeningChanged?.Invoke(this, value);
    }

    protected CancellationToken StartListening()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        SetIsListening(true);

        return _cancellationTokenSource.Token;
    }
}
