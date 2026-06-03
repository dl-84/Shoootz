using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Shoootz.Services.Parser;
using Shoootz.Services.Udp;

namespace Shoootz.Services.Data;

internal class DataProcessor : IDataProcessor, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly IShotDataParser _parser;

    private readonly Channel<byte[]> _udpChannel = Channel.CreateUnbounded<byte[]>();

    private int _errorParsedShotCounter;

    private int _savedToDbShotCounter;

    private int _successParsedShotCounter;

    private int _udpRawShotCounter;

    private int _warmupShotCounter;

    public DataProcessor(IUdpListenerService udpListenerService, IShotDataParser parser)
    {
        _parser = parser;
        udpListenerService.ShotRawDataReceived += OnPacketReceived;
        _ = ProcessUdpChannelReaderLoopAsync(_cancellationTokenSource.Token);
    }

    public int ErrorParsedShotCounter => _errorParsedShotCounter;

    public int SavedToDbShotCounter => _savedToDbShotCounter;

    public int SuccessParsedShotCounter => _successParsedShotCounter;

    public int UdpRawShotCounter => _udpRawShotCounter;

    public int WarmupShotCounter => _warmupShotCounter;

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    private void OnPacketReceived(object? sender, byte[] data)
    {
        Interlocked.Increment(ref _udpRawShotCounter);
        _udpChannel.Writer.TryWrite(data);
    }

    private async Task ProcessUdpChannelReaderLoopAsync(CancellationToken cancellationToken)
    {
        await foreach (byte[] data in _udpChannel.Reader.ReadAllAsync(cancellationToken))
        {
            _parser.Run(data).Match(
                shot =>
                {
                    Interlocked.Increment(ref _successParsedShotCounter);

                    if (shot is null)
                    {
                        Interlocked.Increment(ref _warmupShotCounter);
                    }
                },
                _ => Interlocked.Increment(ref _errorParsedShotCounter)
            );
        }
    }

    private void Mock()
    {
        Interlocked.Increment(ref _savedToDbShotCounter);
    }
}
