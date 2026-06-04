using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Result;
using Result.Types;
using Shoootz.Models.Error;
using Shoootz.Models.Shot;
using Shoootz.Services.Parser;
using Shoootz.Services.Store;
using Shoootz.Services.Udp;

namespace Shoootz.Services.Data;

internal class DataProcessor : IDataProcessor, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly IStoreService _dbService;

    private readonly IShotDataParser _parser;

    private readonly Channel<ShotModel> _shotChannel = Channel.CreateUnbounded<ShotModel>();

    private readonly Channel<byte[]> _udpChannel = Channel.CreateUnbounded<byte[]>();

    private readonly IUdpListenerService _udpListenerService;

    private int _errorParsedShotCounter;

    private int _errorOnSaveShotToDbCounter;

    private int _successParsedShotCounter;

    private int _successSavedShotToDbCounter;

    private int _udpRawShotCounter;

    private int _warmupShotCounter;

    public DataProcessor(IUdpListenerService udpListenerService, IShotDataParser parser, IStoreService dbService)
    {
        _parser = parser;
        _dbService = dbService;
        _udpListenerService = udpListenerService;

        _udpListenerService.ShotRawDataReceived += OnPacketReceived;

        _ = ProcessUdpChannelReaderLoopAsync(_cancellationTokenSource.Token);
        _ = ProcessShotChannelReaderLoopAsync(_cancellationTokenSource.Token);
    }

    public int ErrorParsedShotCounter => _errorParsedShotCounter;

    public int ErrorOnSaveShotToDbCounter => _errorOnSaveShotToDbCounter;

    public int SuccessParsedShotCounter => _successParsedShotCounter;

    public int SuccessSavedShotToDbCounter => _successSavedShotToDbCounter;

    public int UdpRawShotCounter => _udpRawShotCounter;

    public int WarmupShotCounter => _warmupShotCounter;

    public void Dispose()
    {
        _udpListenerService.ShotRawDataReceived -= OnPacketReceived;
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
            Result<ShotModel?, ShotParseError> result = _parser.Run(data);

            result.Match(
                shot =>
                {
                    Interlocked.Increment(ref _successParsedShotCounter);

                    if (shot is null)
                    {
                        Interlocked.Increment(ref _warmupShotCounter);
                    }
                    else
                    {
                        _shotChannel.Writer.TryWrite(shot);
                    }
                },
                error =>
                {
                    // Dont delete this, its for later
                    Interlocked.Increment(ref _errorParsedShotCounter);
                }
            );
        }
    }

    private async Task ProcessShotChannelReaderLoopAsync(CancellationToken cancellationToken)
    {
        await foreach (ShotModel shot in _shotChannel.Reader.ReadAllAsync(cancellationToken))
        {
            Result<Unit, StoreSaveError> result = await _dbService.SaveShotAsync(shot).ConfigureAwait(false);

            result.Match(
                _ =>
                {
                    // Dont delete this, its for later
                    Interlocked.Increment(ref _successSavedShotToDbCounter);
                },
                _ =>
                {
                    // Dont delete this, its for later
                    Interlocked.Increment(ref _errorOnSaveShotToDbCounter);
                }
            );
        }
    }
}
