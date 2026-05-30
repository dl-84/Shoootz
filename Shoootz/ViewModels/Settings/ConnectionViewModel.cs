using System;
using System.Collections.Generic;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Database;
using Shoootz.Services.App;
using Shoootz.Services.Database;
using Shoootz.Services.Grafik;
using Shoootz.Services.Settings;

namespace Shoootz.ViewModels.Settings;

internal partial class ConnectionViewModel : ViewModelBase
{
    private readonly IDbConnectionTester _connectionTester;

    private readonly ISettingsService _settingsService;

    private readonly SettingsModel _settings;

    public ConnectionViewModel(
        IDbConnectionTester connectionTester,
        IGrafikService grafikService,
        SettingsModel settings,
        ISettingsService settingsService
    )
    {
        _connectionTester = connectionTester;
        _settings = settings;
        _settingsService = settingsService;

        AutoConnect = settings.UdpConnectionModel.AutoConnect;
        ConnectionString = settings.DbConnectionModel.ConnectionString;
        IpAddress = settings.UdpConnectionModel.IpAddress;
        Port = settings.UdpConnectionModel.Port.ToString();
        SelectedProvider = settings.DbConnectionModel.ProviderType;
        HeartPulseIcon = grafikService.GetIcon(AppIcon.HeartPulse, AppBrush.PrimaryForeground);
    }

    public event Action<bool, string?>? ConnectionTestCompleted;

    public event Action<SettingsModel>? SettingsSaved;

    public IImage HeartPulseIcon { get; }

    public IEnumerable<ProviderType> ProviderOptions { get; } = Enum.GetValues<ProviderType>();

    [ObservableProperty]
    public partial string ConnectionString { get; set; }

    [ObservableProperty]
    public partial bool AutoConnect { get; set; }

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
    private void TestConnection()
    {
        DbConnectionModel dbConnection = new DbConnectionModel
        {
            ConnectionString = ConnectionString,
            ProviderType = SelectedProvider,
        };

        _connectionTester
            .Run(dbConnection)
            .Match<object?>(
                _ =>
                {
                    ConnectionTestCompleted?.Invoke(true, null);
                    return null;
                },
                error =>
                {
                    ConnectionTestCompleted?.Invoke(false, error.Value.Message);
                    return null;
                }
            );
    }
}
