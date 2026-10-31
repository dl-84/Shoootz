using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Result;
using Result.Types;
using Shoootz.Models.Error;
using Shoootz.Models.Settings;

namespace Shoootz.Services.Receiver;

internal class UdpReceiverService(IOptionsMonitor<SettingsModel> settings) : ReceiverServiceBase
{
    private UdpClient? _client;

    public override Result<Unit, ReceiverConnectionErrorModel> Connect()
    {
        Disconnect();

        try
        {
            UdpClient client = new();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, settings.CurrentValue.Receiver.Port));
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
            using UdpClient client = new(settings.CurrentValue.Receiver.Port);
            return true;
        }
        catch (Exception exception)
        {
            return new Error<ReceiverConnectionErrorModel>(new ReceiverConnectionErrorModel(exception.Message));
        }
    }

    private async Task ReceiveLoopAsync(UdpClient client, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                UdpReceiveResult result = await client.ReceiveAsync(cancellationToken);

                RaiseShotRawDataReceived(result.Buffer);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                // Transient network errors (e.g. ICMP port-unreachable) are expected on UDP — keep listening.
            }
            catch (Exception)
            {
                // A single bad packet or subscriber error must not permanently kill the listener.
            }
        }

        SetIsListening(false);
    }
}
