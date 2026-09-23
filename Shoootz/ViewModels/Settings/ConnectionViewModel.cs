using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Result;
using Shoootz.Models.Database;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Database;
using Shoootz.Resources.Lang;
using Shoootz.Services.App;
using Shoootz.Services.Data;
using Shoootz.Services.Localization;
using Shoootz.Services.Settings;
using Shoootz.Services.Store;
using Shoootz.Services.Udp;
using Shoootz.Store;
using Shoootz.Store.Services;

namespace Shoootz.ViewModels.Settings;

internal partial class ConnectionViewModel : ViewModelBase
{
    private readonly IConnectionTester _connectionTester;

    private readonly IDataProcessor _dataProcessor;

    private readonly SettingsModel _settings;

    private readonly ISettingsWriter _settingsWriter;

    private readonly IStoreService _storeService;

    private readonly IUdpListenerService _udpListenerService;

    public ConnectionViewModel(
        IConnectionTester connectionTester,
        IDataProcessor dataProcessor,
        IOptionsMonitor<SettingsModel> settings,
        ISettingsWriter settingsWriter,
        IStoreService storeService,
        IUdpListenerService udpListenerService
    )
    {
        _connectionTester = connectionTester;
        _dataProcessor = dataProcessor;
        _settings = settings.CurrentValue;
        _settingsWriter = settingsWriter;
        _storeService = storeService;
        _udpListenerService = udpListenerService;

        AutoConnect = _settings.UdpConnection.AutoConnect;
        IsUdpListening = udpListenerService.IsListening;
        Port = _settings.UdpConnection.Port.ToString();
        SelectedProvider = _settings.DatabaseConnection.Provider;
        ConnectionString = _settings.DatabaseConnection.ConnectionString;

        _udpListenerService.IsListeningChanged += (_, isListening) => IsUdpListening = isListening;

        IsDbConnected = _storeService.IsConnected;
        _storeService.ConnectionChanged += (_, _) => IsDbConnected = _storeService.IsConnected;

        _ = RefreshDbStatusAsync();
    }

    public event Action<bool, string?>? ConnectionTestCompleted;

    public event Action<bool, string?>? DbInitializeCompleted;

    public event Action? DbInitializeStarted;

    public IEnumerable<ProviderType> ProviderOptions { get; } = Enum.GetValues<ProviderType>();

    [ObservableProperty]
    public partial string ConnectionString { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InitializeDbLabel))]
    [NotifyPropertyChangedFor(nameof(IsInitializeDbEnabled))]
    public partial DbStatus DbStatus { get; set; }

    [ObservableProperty]
    public partial bool AutoConnect { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InitializeDbLabel))]
    [NotifyPropertyChangedFor(nameof(IsInitializeDbEnabled))]
    public partial bool IsDbConnected { get; set; }

    [ObservableProperty]
    public partial bool IsUdpListening { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPortValid))]
    public partial string Port { get; set; }

    public string InitializeDbLabel =>
        !IsDbConnected
            ? LocalizationService.Instance[Messages.DbNotConnected]
            : DbStatus switch
            {
                DbStatus.UpdateAvailable => LocalizationService.Instance[Messages.UpdateDb],
                DbStatus.UpToDate => LocalizationService.Instance[Messages.DbUpToDate],
                _ => LocalizationService.Instance[Messages.InitializeDb],
            };

    public bool IsInitializeDbEnabled => DbStatus != DbStatus.UpToDate && IsDbConnected;

    public bool IsPortValid => int.TryParse(Port, out int port) && port is >= 1 and <= 65535;

    public bool IsSaveEnabled => CanSave();

    [ObservableProperty]
    public partial ProviderType SelectedProvider { get; set; }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        _settings.DatabaseConnection.ConnectionString = ConnectionString;
        _settings.DatabaseConnection.Provider = SelectedProvider;
        _settings.UdpConnection.AutoConnect = AutoConnect;
        _settings.UdpConnection.Port = int.Parse(Port);
        _settingsWriter.Save(_settings);

        NotifySaveStateChanged();
    }

    private bool CanSave() =>
        int.TryParse(Port, out int port)
        && port is >= 1 and <= 65535
        && (
            AutoConnect != _settings.UdpConnection.AutoConnect
            || ConnectionString != _settings.DatabaseConnection.ConnectionString
            || Port != _settings.UdpConnection.Port.ToString()
            || SelectedProvider != _settings.DatabaseConnection.Provider
        );

    private void NotifySaveStateChanged()
    {
        SaveCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(IsSaveEnabled));
    }

    partial void OnAutoConnectChanged(bool value) => NotifySaveStateChanged();

    partial void OnConnectionStringChanged(string value) => NotifySaveStateChanged();

    partial void OnPortChanged(string value) => NotifySaveStateChanged();

    partial void OnSelectedProviderChanged(ProviderType value)
    {
        ConnectionString = value switch
        {
#if DEBUG
            ProviderType.PostgreSql =>
                "Host=192.168.10.9;Port=5433;Database=Shoootz;Username=shoootz_user;Password=shoootz_passwort",
#else
            ProviderType.PostgreSql =>
                "Host=<Host IP-Address>;Port=5432;Database=Shoootz;Username=<Username>;Password=<Password>",
#endif
            _ => $"Data Source={AppPath.DbFile}",
        };

        NotifySaveStateChanged();
    }

    [RelayCommand]
    private async Task ConnectDb()
    {
        Result<bool, DbConnectionError> result = SelectedProvider switch
        {
            ProviderType.PostgreSql => _connectionTester.PostgreSql(ConnectionString),
            _ => _connectionTester.Sqlite(ConnectionString),
        };

        await result.Match(
            async _ =>
            {
                _storeService.Connect();
                await RefreshDbStatusAsync();
            },
            error =>
            {
                ConnectionTestCompleted?.Invoke(false, error.Value.Message);
                return Task.CompletedTask;
            }
        );
    }

    [RelayCommand]
    private void DisconnectDb() => _storeService.Disconnect();

    [RelayCommand]
    private void StartUdp()
    {
        if (int.TryParse(Port, out int port))
        {
            _dataProcessor.Start();
            _udpListenerService.Start(port);
        }
    }

    [RelayCommand]
    private void StopUdp() => _udpListenerService.Stop();

    [RelayCommand]
    private void TestUdpConnection()
    {
        if (!int.TryParse(Port, out int port))
        {
            return;
        }

        _udpListenerService
            .TestConnection(port)
            .Match(
                _ => ConnectionTestCompleted?.Invoke(true, null),
                error => ConnectionTestCompleted?.Invoke(false, error.Value.Message)
            );
    }

    [RelayCommand(CanExecute = nameof(CanInitializeDb))]
    private async Task InitializeDb()
    {
        DbInitializeStarted?.Invoke();

        try
        {
            await _storeService.InitializeAsync();

            await Task.Delay(2000);

            DbStatus = await _storeService.GetDbStatusAsync();
            InitializeDbCommand.NotifyCanExecuteChanged();
            DbInitializeCompleted?.Invoke(true, null);
        }
        catch (Exception exception)
        {
            DbInitializeCompleted?.Invoke(false, exception.Message);
        }
    }

    private bool CanInitializeDb() => DbStatus != DbStatus.UpToDate;

    private async Task RefreshDbStatusAsync()
    {
        DbStatus = await _storeService.GetDbStatusAsync();
        InitializeDbCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void TestDbConnection()
    {
        Result<bool, DbConnectionError> result = SelectedProvider switch
        {
            ProviderType.PostgreSql => _connectionTester.PostgreSql(ConnectionString),
            _ => _connectionTester.Sqlite(ConnectionString),
        };

        result.Match(
            _ => ConnectionTestCompleted?.Invoke(true, null),
            error => ConnectionTestCompleted?.Invoke(false, error.Value.Message)
        );
    }
}
