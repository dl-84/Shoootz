using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Result;
using Shoootz.Models.Database;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Database;
using Shoootz.Services.App;
using Shoootz.Services.Graphics;
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

    private readonly SettingsModel _settings;

    private readonly ISettingsWriter _settingsWriter;

    private readonly IStoreService _storeService;

    private readonly IUdpListenerService _udpListenerService;

    public ConnectionViewModel(
        IConnectionTester connectionTester,
        IGraphicsService grafikService,
        IOptionsMonitor<SettingsModel> settings,
        ISettingsWriter settingsWriter,
        IStoreService storeService,
        IUdpListenerService udpListenerService
    )
    {
        _connectionTester = connectionTester;
        _settings = settings.CurrentValue;
        _settingsWriter = settingsWriter;
        _storeService = storeService;
        _udpListenerService = udpListenerService;

        AutoConnect = _settings.UdpConnection.AutoConnect;
        ConnectionString = _settings.DatabaseConnection.ConnectionString;
        IsUdpListening = udpListenerService.IsListening;
        Port = _settings.UdpConnection.Port.ToString();
        SelectedProvider = _settings.DatabaseConnection.Provider;
        HeartPulseIcon = grafikService.GetIcon(AppIcon.HeartPulse, AppBrush.PrimaryForeground);

        _udpListenerService.IsListeningChanged += (_, isListening) => IsUdpListening = isListening;

        _ = RefreshDbStatusAsync();
    }

    public event Action<bool, string?>? ConnectionTestCompleted;

    public event Action<bool, string?>? DbInitializeCompleted;

    public event Action? DbInitializeStarted;

    public IImage HeartPulseIcon { get; }

    public IEnumerable<ProviderType> ProviderOptions { get; } = Enum.GetValues<ProviderType>();

    [ObservableProperty]
    public partial string ConnectionString { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InitializeDbLabel))]
    public partial DbStatus DbStatus { get; set; }

    [ObservableProperty]
    public partial bool AutoConnect { get; set; }

    [ObservableProperty]
    public partial bool IsUdpListening { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPortValid))]
    public partial string Port { get; set; }

    public string InitializeDbLabel =>
        DbStatus switch
        {
            DbStatus.UpdateAvailable => LocalizationService.Instance["UpdateDb"],
            DbStatus.UpToDate => LocalizationService.Instance["DbUpToDate"],
            _ => LocalizationService.Instance["InitializeDb"],
        };

    public bool IsPortValid => int.TryParse(Port, out int port) && port is >= 1 and <= 65535;

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

        SaveCommand.NotifyCanExecuteChanged();
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

    partial void OnAutoConnectChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();

    partial void OnConnectionStringChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

    partial void OnPortChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

    partial void OnSelectedProviderChanged(ProviderType value) => SaveCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private void StartUdp()
    {
        if (int.TryParse(Port, out int port))
        {
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

            // Dont delete, its for the waiting dialog.
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
