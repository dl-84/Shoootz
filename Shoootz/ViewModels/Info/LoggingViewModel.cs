using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Shoootz.Common.ViewModels;
using Shoootz.Models.Udp;
using Shoootz.Services.Data;

namespace Shoootz.ViewModels.Info;

internal partial class LoggingViewModel : ViewModelBase, IDisposable
{
    private readonly IDataProcessor _dataProcessor;

    private readonly DispatcherTimer _refreshTimer;

    [ObservableProperty]
    private int _incorrectNumberOfObjects;

    [ObservableProperty]
    private int _missingShooterData;

    [ObservableProperty]
    private int _notTypeOfShot;

    [ObservableProperty]
    private int _parsedShots;

    [ObservableProperty]
    private int _parsedShotErrors;

    [ObservableProperty]
    private int _receivedShots;

    [ObservableProperty]
    private int _savedShots;

    [ObservableProperty]
    private int _savedShotErrors;

    [ObservableProperty]
    private int _warmupShots;

    public LoggingViewModel(IDataProcessor dataProcessor)
    {
        _dataProcessor = dataProcessor;

        foreach (UdpRawModel entry in _dataProcessor.RawLogEntries)
        {
            Logs.Insert(0, entry);
        }

        _dataProcessor.RawDataLogged += OnRawDataLogged;

        OnTimerTick(this, EventArgs.Empty);

        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _refreshTimer.Tick += OnTimerTick;
        _refreshTimer.Start();
    }

    public ObservableCollection<UdpRawModel> Logs { get; } = [];

    public void Dispose()
    {
        _refreshTimer.Stop();
        _refreshTimer.Tick -= OnTimerTick;
        _dataProcessor.RawDataLogged -= OnRawDataLogged;
    }

    private void OnRawDataLogged(object? sender, UdpRawModel entry)
    {
        Dispatcher.UIThread.Post(() => Logs.Insert(0, entry));
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        IncorrectNumberOfObjects = _dataProcessor.IncorrectNumberOfObjectsCounter;
        MissingShooterData = _dataProcessor.MissingShooterDataCounter;
        NotTypeOfShot = _dataProcessor.NotTypeOfShotCounter;
        ParsedShots = _dataProcessor.SuccessParsedShotCounter;
        ParsedShotErrors = _dataProcessor.ErrorParsedShotCounter;
        ReceivedShots = _dataProcessor.UdpRawShotCounter;
        SavedShots = _dataProcessor.SuccessSavedShotToDbCounter;
        SavedShotErrors = _dataProcessor.ErrorOnSaveShotToDbCounter;
        WarmupShots = _dataProcessor.WarmupShotCounter;
    }
}
