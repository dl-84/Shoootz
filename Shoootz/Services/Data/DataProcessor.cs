using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Result;
using Result.Types;
using Shoootz.Models.Error;
using Shoootz.Models.Shot;
using Shoootz.Models.Udp;
using Shoootz.Services.Parser;
using Shoootz.Services.Store;
using Shoootz.Services.Udp;

namespace Shoootz.Services.Data;

internal class DataProcessor(IUdpListenerService udpListenerService, IShotDataParser parser, IStoreService dbService)
    : IDataProcessor,
        IDisposable
{
    private const int MaxRawLogEntries = 300;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly ConcurrentDictionary<ShotCategoryType, int> _counters = new();

    private readonly List<UdpRawModel> _rawLog = [];

    private readonly Lock _rawLogLock = new();

    private readonly Channel<ShotModel> _shotChannel = Channel.CreateUnbounded<ShotModel>();

    private readonly Channel<string> _udpChannel = Channel.CreateUnbounded<string>();

    private bool _started;

    public event EventHandler<UdpRawModel>? RawDataLogged;

    public int ErrorOnSaveShotToDbCounter => _counters.GetValueOrDefault(ShotCategoryType.SavedError);

    public int ErrorParsedShotCounter =>
        _counters.GetValueOrDefault(ShotCategoryType.Exception)
        + _counters.GetValueOrDefault(ShotCategoryType.MessageIsNull);

    public int IncorrectNumberOfObjectsCounter =>
        _counters.GetValueOrDefault(ShotCategoryType.IncorrectNumberOfObjects);

    public int MissingShooterDataCounter => _counters.GetValueOrDefault(ShotCategoryType.HasMissingShooterData);

    public int NotTypeOfShotCounter => _counters.GetValueOrDefault(ShotCategoryType.IsNotTypeOfShot);

    public IReadOnlyList<UdpRawModel> RawLogEntries
    {
        get
        {
            lock (_rawLogLock)
            {
                return [.. _rawLog];
            }
        }
    }

    public int SuccessParsedShotCounter => _counters.GetValueOrDefault(ShotCategoryType.ParsedSuccess);

    public int SuccessSavedShotToDbCounter => _counters.GetValueOrDefault(ShotCategoryType.SavedSuccess);

    public int UdpRawShotCounter => _counters.GetValueOrDefault(ShotCategoryType.Received);

    public int WarmupShotCounter => _counters.GetValueOrDefault(ShotCategoryType.IsWarmup);

    public void Dispose()
    {
        udpListenerService.ShotRawDataReceived -= OnPacketReceived;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    public void Start()
    {
        if (_started)
        {
            return;
        }

        udpListenerService.ShotRawDataReceived += OnPacketReceived;

        _ = ProcessUdpChannelReaderLoopAsync(_cancellationTokenSource.Token);
        _ = ProcessShotChannelReaderLoopAsync(_cancellationTokenSource.Token);
        _started = true;
    }

    private void IncrementCount(ShotCategoryType category)
    {
        _counters.AddOrUpdate(category, 1, static (_, count) => count + 1);
    }

    private void OnPacketReceived(object? sender, byte[] data)
    {
        IncrementCount(ShotCategoryType.Received);

        string rawData = Encoding.UTF8.GetString(data);
        _udpChannel.Writer.TryWrite(rawData);

        UdpRawModel entry = new(DateTime.Now, rawData);

        lock (_rawLogLock)
        {
            _rawLog.Add(entry);

            if (_rawLog.Count > MaxRawLogEntries)
            {
                _rawLog.RemoveAt(0);
            }
        }

        RawDataLogged?.Invoke(this, entry);
    }

    private async Task ProcessUdpChannelReaderLoopAsync(CancellationToken cancellationToken)
    {
        await foreach (string rawData in _udpChannel.Reader.ReadAllAsync(cancellationToken))
        {
            Result<ShotModel, ShotCategoryType> result = parser.Run(rawData);

            result.Match(
                shot =>
                {
                    IncrementCount(ShotCategoryType.ParsedSuccess);
                    _shotChannel.Writer.TryWrite(shot);
                },
                error => IncrementCount(error.Value)
            );
        }
    }

    private async Task ProcessShotChannelReaderLoopAsync(CancellationToken cancellationToken)
    {
        await foreach (ShotModel shot in _shotChannel.Reader.ReadAllAsync(cancellationToken))
        {
            if (!dbService.IsConnected)
            {
                IncrementCount(ShotCategoryType.SavedError);
                continue;
            }

            Result<Unit, StoreSaveErrorModel> result = await dbService.SaveShotAsync(shot).ConfigureAwait(false);

            result.Match(
                _ => IncrementCount(ShotCategoryType.SavedSuccess),
                _ => IncrementCount(ShotCategoryType.SavedError)
            );
        }
    }
}
