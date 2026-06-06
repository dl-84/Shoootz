using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Result;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Database;
using Shoootz.Services.App;
using Shoootz.Services.Graphics;
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

    private readonly ISettingsService _settingsService;

    private readonly IStoreService _storeService;

    private readonly IUdpListenerService _udpListenerService;

    public ConnectionViewModel(
        IConnectionTester connectionTester,
        IGraphicsService grafikService,
        SettingsModel settings,
        ISettingsService settingsService,
        IStoreService storeService,
        IUdpListenerService udpListenerService
    )
    {
        _connectionTester = connectionTester;
        _settings = settings;
        _settingsService = settingsService;
        _storeService = storeService;
        _udpListenerService = udpListenerService;

        AutoConnect = settings.UdpConnectionModel.AutoConnect;
        ConnectionString = settings.DbConnectionModel.ConnectionString;
        IpAddress = settings.UdpConnectionModel.IpAddress;
        IsUdpListening = udpListenerService.IsListening;
        Port = settings.UdpConnectionModel.Port.ToString();
        SelectedProvider = settings.DbConnectionModel.ProviderType;
        HeartPulseIcon = grafikService.GetIcon(AppIcon.HeartPulse, AppBrush.PrimaryForeground);

        _udpListenerService.IsListeningChanged += (_, isListening) => IsUdpListening = isListening;
    }

    public event Action<bool, string?>? ConnectionTestCompleted;

    public event Action<bool, string?>? DbInitializeCompleted;

    public event Action? DbInitializeStarted;

    public event Action<SettingsModel>? SettingsSaved;

    public IImage HeartPulseIcon { get; }

    public IEnumerable<ProviderType> ProviderOptions { get; } = Enum.GetValues<ProviderType>();

    [ObservableProperty]
    public partial string ConnectionString { get; set; }

    [ObservableProperty]
    public partial bool AutoConnect { get; set; }

    [ObservableProperty]
    public partial bool IsUdpListening { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIpAddressValid))]
    public partial string IpAddress { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPortValid))]
    public partial string Port { get; set; }

    public bool IsIpAddressValid => System.Net.IPAddress.TryParse(IpAddress, out _);

    public bool IsPortValid => int.TryParse(Port, out int port) && port is >= 1 and <= 65535;

    [ObservableProperty]
    public partial ProviderType SelectedProvider { get; set; }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        _settings.DbConnectionModel.ConnectionString = ConnectionString;
        _settings.DbConnectionModel.ProviderType = SelectedProvider;
        _settings.UdpConnectionModel.AutoConnect = AutoConnect;
        _settings.UdpConnectionModel.IpAddress = IpAddress;
        _settings.UdpConnectionModel.Port = int.Parse(Port);
        _settingsService.Save(_settings);

        SettingsSaved?.Invoke(_settings);
        SaveCommand.NotifyCanExecuteChanged();
    }

    private bool CanSave() =>
        int.TryParse(Port, out int port)
        && port is >= 1 and <= 65535
        && (
            AutoConnect != _settings.UdpConnectionModel.AutoConnect
            || ConnectionString != _settings.DbConnectionModel.ConnectionString
            || IpAddress != _settings.UdpConnectionModel.IpAddress
            || Port != _settings.UdpConnectionModel.Port.ToString()
            || SelectedProvider != _settings.DbConnectionModel.ProviderType
        );

    partial void OnAutoConnectChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();

    partial void OnConnectionStringChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

    partial void OnIpAddressChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

    partial void OnPortChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

    partial void OnSelectedProviderChanged(ProviderType value) => SaveCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private void StartUdp()
    {
        if (int.TryParse(Port, out int port))
        {
            _udpListenerService.Start(IpAddress, port);
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

    [RelayCommand]
    private async Task InitializeDb()
    {
        DbInitializeStarted?.Invoke();

        try
        {
            await _storeService.InitializeAsync();

            // Dont delete, its for the waiting dialog.
            await Task.Delay(2000);

            DbInitializeCompleted?.Invoke(true, null);
        }
        catch (Exception exception)
        {
            DbInitializeCompleted?.Invoke(false, exception.Message);
        }
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
