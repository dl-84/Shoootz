using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Result;
using Result.Types;
using Shoootz.Models.Error;
using Shoootz.Models.Settings;

namespace Shoootz.Services.Receiver;

internal class TcpReceiverService(IOptionsMonitor<SettingsModel> settings) : ReceiverServiceBase
{
    private TcpClient? _client;

    public override Result<Unit, ReceiverConnectionErrorModel> Connect()
    {
        Disconnect();

        try
        {
            TcpClient client = new();
            client.Connect(settings.CurrentValue.Receiver.Host ?? string.Empty, settings.CurrentValue.Receiver.Port);
            _client = client;

            CancellationToken cancellationToken = StartListening();
            _ = ReceiveLoopAsync(client, cancellationToken);

            return Unit.Value;
        }
        catch (Exception exception)
        {
            SetIsListening(false);
            return new Error<ReceiverConnectionErrorModel>(new ReceiverConnectionErrorModel(exception.Message));
        }
    }

    public override void Disconnect()
    {
        _client?.Dispose();
        _client = null;
        base.Disconnect();
    }

    public override Result<bool, ReceiverConnectionErrorModel> TestConnection()
    {
        try
        {
            using TcpClient client = new();
            client.Connect(settings.CurrentValue.Receiver.Host ?? string.Empty, settings.CurrentValue.Receiver.Port);

            return true;
        }
        catch (Exception exception)
        {
            return new Error<ReceiverConnectionErrorModel>(new ReceiverConnectionErrorModel(exception.Message));
        }
    }

    private async Task ReceiveLoopAsync(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            byte[] lengthBuffer = new byte[4];

            while (!cancellationToken.IsCancellationRequested)
            {
                await stream.ReadExactlyAsync(lengthBuffer, cancellationToken);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(lengthBuffer);
                }

                byte[] payload = new byte[BitConverter.ToInt32(lengthBuffer, 0)];
                await stream.ReadExactlyAsync(payload, cancellationToken);

                RaiseShotRawDataReceived(payload);
            }
        }
        catch (OperationCanceledException)
        {
            // Disconnect() was called.
        }
        catch (Exception)
        {
            // Connection dropped - fall through to clean up below.
        }
        finally
        {
            SetIsListening(false);
        }
    }
}
