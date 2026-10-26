using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using Shoootz.Bridge.TCP.Models.Settings;
using Shoootz.Common.Extensions;

namespace Shoootz.Bridge.TCP.Services.Bridge;

internal partial class BridgeService : ObservableObject, IBridgeService
{
    private readonly ConcurrentDictionary<Guid, NetworkStream> _clients = new();

    private CancellationTokenSource? _cancellationTokenSource;

    public BridgeService(IOptionsMonitor<SettingsModel> settings)
    {
        TcpPort = settings.CurrentValue.TcpPort.ToString();
        UdpPort = settings.CurrentValue.UdpPort.ToString();
    }

    [ObservableProperty]
    public partial int ConnectedClientCount { get; set; }

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    public bool IsStartEnabled => IsTcpPortValid && IsUdpPortValid;

    public bool IsTcpPortValid => TcpPort.IsValidPort();

    public bool IsUdpPortValid => UdpPort.IsValidPort();

    [ObservableProperty]
    public partial string? LastError { get; set; }

    [ObservableProperty]
    public partial int PacketsRelayedCount { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTcpPortValid))]
    [NotifyPropertyChangedFor(nameof(IsStartEnabled))]
    public partial string TcpPort { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUdpPortValid))]
    [NotifyPropertyChangedFor(nameof(IsStartEnabled))]
    public partial string UdpPort { get; set; }

    public void Start()
    {
        Stop();

        LastError = null;

        int tcpPort = int.Parse(TcpPort);
        int udpPort = int.Parse(UdpPort);

        CancellationTokenSource cancellationTokenSource = new();
        _cancellationTokenSource = cancellationTokenSource;

        Task tcpTask = RunOrFailAsync(
            "TCP-Listener",
            () => AcceptTcpClientsAsync(tcpPort, cancellationTokenSource.Token),
            cancellationTokenSource
        );

        Task udpTask = RunOrFailAsync(
            "UDP-Listener",
            () => RelayUdpToTcpAsync(udpPort, cancellationTokenSource.Token),
            cancellationTokenSource
        );

        IsRunning = true;

        _ = Task.WhenAll(tcpTask, udpTask)
            .ContinueWith(_ => Dispatcher.UIThread.Post(() => IsRunning = false), cancellationTokenSource.Token);
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;

        foreach (NetworkStream stream in _clients.Values)
        {
            stream.Dispose();
        }

        _clients.Clear();
        ConnectedClientCount = 0;
        IsRunning = false;
    }

    private async Task AcceptTcpClientsAsync(int port, CancellationToken cancellationToken)
    {
        TcpListener listener = new(IPAddress.Any, port);
        listener.Start();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient tcpClient;

                try
                {
                    tcpClient = await listener.AcceptTcpClientAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                Guid clientId = Guid.NewGuid();
                NetworkStream stream = tcpClient.GetStream();
                _clients[clientId] = stream;

                int connectedCount = _clients.Count;
                Dispatcher.UIThread.Post(() => ConnectedClientCount = connectedCount);

                _ = WatchForDisconnectAsync(tcpClient, clientId, cancellationToken);
            }
        }
        finally
        {
            listener.Stop();
        }
    }

    private async Task RelayUdpToTcpAsync(int port, CancellationToken cancellationToken)
    {
        using UdpClient udpClient = new();
        udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));

        int packetsRelayed = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            UdpReceiveResult result;

            try
            {
                result = await udpClient.ReceiveAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                continue;
            }

            byte[] lengthPrefix = BitConverter.GetBytes(result.Buffer.Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthPrefix);
            }

            foreach ((Guid clientId, NetworkStream stream) in _clients)
            {
                try
                {
                    await stream.WriteAsync(lengthPrefix, cancellationToken);
                    await stream.WriteAsync(result.Buffer, cancellationToken);
                }
                catch (Exception)
                {
                    _clients.TryRemove(clientId, out _);
                }
            }

            packetsRelayed++;
            int relayedCount = packetsRelayed;
            Dispatcher.UIThread.Post(() => PacketsRelayedCount = relayedCount);
        }
    }

    private async Task RunOrFailAsync(string name, Func<Task> action, CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            await action();
        }
        catch (OperationCanceledException)
        {
            // Nothing todo
        }
        catch (Exception exception)
        {
            string message = $"{name}: {exception.Message}";
            Dispatcher.UIThread.Post(() => LastError = message);
            await cancellationTokenSource.CancelAsync();
        }
    }

    private async Task WatchForDisconnectAsync(TcpClient tcpClient, Guid clientId, CancellationToken cancellationToken)
    {
        try
        {
            byte[] buffer = new byte[1];

            // Only used to detect disconnect (0 bytes / exception) — the actual byte count is irrelevant.
            _ = await tcpClient.GetStream().ReadAsync(buffer, cancellationToken);
        }
        catch (Exception)
        {
            // Client gone — fall through to clean up below.
        }

        _clients.TryRemove(clientId, out _);
        tcpClient.Dispose();

        int connectedCount = _clients.Count;
        Dispatcher.UIThread.Post(() => ConnectedClientCount = connectedCount);
    }
}
