using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Result;
using Result.Types;
using Shoootz.Common.Extensions;
using Shoootz.Common.Resources.Lang;
using Shoootz.Common.Services.App;
using Shoootz.Common.Services.Language;
using Shoootz.Common.Services.Localization;
using Shoootz.Common.Services.Settings;
using Shoootz.Common.ViewModels;
using Shoootz.Common.ViewModels.Settings;
using Shoootz.Models.Database;
using Shoootz.Models.Error;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Incoming;
using Shoootz.Services.Data;
using Shoootz.Services.Store;
using Shoootz.Services.Udp;
using Shoootz.Store;
using Shoootz.Store.Services;
using Shoootz.Store.Types;

namespace Shoootz.ViewModels.Settings;

internal partial class ConnectionViewModel : ViewModelBase, IDisposable, IHasUnsavedChanges
{
    private readonly IConnectionTester _connectionTester;

    private readonly IDataProcessor _dataProcessor;

    private readonly SettingsModel _settings;

    private readonly ISettingsWriter<SettingsModel> _settingsWriter;

    private readonly IStoreService _storeService;

    private readonly IUdpListenerService _udpListenerService;

    public ConnectionViewModel(
        IConnectionTester connectionTester,
        IDataProcessor dataProcessor,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IOptionsMonitor<SettingsModel> settings,
        ISettingsWriter<SettingsModel> settingsWriter,
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

        GeneralSettings = new GeneralSettingsViewModel<SettingsModel>(
            languageService,
            localizationService,
            settings,
            settingsWriter,
            additionalDeleteFolderCleanup: async () =>
            {
                Result<Unit, StoreSaveErrorModel> result = await _storeService.DeleteDatabaseAsync();

                return result.Match(
                    _ => null!,
                    error =>
                        $"{LocalizationService.Instance[Messages.DeleteDatabaseFailedHint]}\n\n{error.Value.Message}"
                );
            }
        );

        AutoConnect = _settings.Receiver.AutoConnect;
        Host = _settings.Receiver.Host ?? string.Empty;
        IsUdpListening = udpListenerService.IsListening;
        Port = _settings.Receiver.Port.ToString();
        SelectedProvider = _settings.DatabaseConnection.Provider;
        SelectedReceiverType = _settings.Receiver.Type;
        ConnectionString = _settings.DatabaseConnection.ConnectionString;

        _udpListenerService.IsListeningChanged += OnUdpListeningChanged;

        IsDbConnected = _storeService.IsConnected;
        _storeService.ConnectionChanged += OnStoreConnectionChanged;

        _ = RefreshDbStatusAsync();
    }

    public event Action<bool, string?>? ConnectionTestCompleted;

    public event Action<bool, string?>? DbInitializeCompleted;

    public event Action? DbInitializeStarted;

    [ObservableProperty]
    public partial bool AutoConnect { get; set; }

    public string ConnectionErrorHint => !IsHostValid ? HostErrorHint : PortErrorHint;

    [ObservableProperty]
    public partial string ConnectionString { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InitializeDbLabel))]
    [NotifyPropertyChangedFor(nameof(IsInitializeDbEnabled))]
    public partial DbStatusType DbStatusType { get; set; }

    public GeneralSettingsViewModel<SettingsModel> GeneralSettings { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectionErrorHint))]
    [NotifyPropertyChangedFor(nameof(HostErrorHint))]
    [NotifyPropertyChangedFor(nameof(IsHostValid))]
    public partial string Host { get; set; }

    public string HostErrorHint =>
        IsHostValid ? string.Empty : LocalizationService.Instance[nameof(Messages.HostInvalidHint)];

    public string InitializeDbLabel =>
        !IsDbConnected
            ? LocalizationService.Instance[Messages.DbNotConnected]
            : DbStatusType switch
            {
                DbStatusType.UpdateAvailable => LocalizationService.Instance[Messages.UpdateDb],
                DbStatusType.UpToDate => LocalizationService.Instance[Messages.DbUpToDate],
                _ => LocalizationService.Instance[Messages.InitializeDb],
            };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InitializeDbLabel))]
    [NotifyPropertyChangedFor(nameof(IsInitializeDbEnabled))]
    public partial bool IsDbConnected { get; set; }

    public bool IsHostValid => !IsTcpSelected || Host.IsValidIPv4();

    public bool IsInitializeDbEnabled => DbStatusType != DbStatusType.UpToDate && IsDbConnected;

    public bool IsPortValid => Port.IsValidPort();

    public bool IsSaveEnabled => CanSave();

    public bool IsTcpSelected => SelectedReceiverType == ReceiverType.Tcp;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UdpStatusLabel))]
    public partial bool IsUdpListening { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectionErrorHint))]
    [NotifyPropertyChangedFor(nameof(IsPortValid))]
    [NotifyPropertyChangedFor(nameof(PortErrorHint))]
    public partial string Port { get; set; }

    public string PortErrorHint =>
        IsPortValid ? string.Empty : LocalizationService.Instance[nameof(Messages.PortRangeHint)];

    public IEnumerable<ProviderType> ProviderOptions { get; } = Enum.GetValues<ProviderType>();

    public IEnumerable<ReceiverType> ReceiverTypeOptions { get; } = Enum.GetValues<ReceiverType>();

    [ObservableProperty]
    public partial ProviderType SelectedProvider { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectionErrorHint))]
    [NotifyPropertyChangedFor(nameof(HostErrorHint))]
    [NotifyPropertyChangedFor(nameof(IsHostValid))]
    [NotifyPropertyChangedFor(nameof(IsTcpSelected))]
    public partial ReceiverType SelectedReceiverType { get; set; }

    public string UdpStatusLabel =>
        LocalizationService.Instance[IsUdpListening ? nameof(Messages.Connected) : nameof(Messages.NotConnected)];

    public void Dispose()
    {
        _udpListenerService.IsListeningChanged -= OnUdpListeningChanged;
        _storeService.ConnectionChanged -= OnStoreConnectionChanged;
    }

    private bool CanInitializeDb() => IsInitializeDbEnabled;

    private bool CanSave() =>
        Port.IsValidPort()
        && IsHostValid
        && (
            AutoConnect != _settings.Receiver.AutoConnect
            || ConnectionString != _settings.DatabaseConnection.ConnectionString
            || (SelectedReceiverType == ReceiverType.Tcp && Host != (_settings.Receiver.Host ?? string.Empty))
            || Port != _settings.Receiver.Port.ToString()
            || SelectedProvider != _settings.DatabaseConnection.Provider
            || SelectedReceiverType != _settings.Receiver.Type
        );

    [RelayCommand]
    private async Task ConnectDb()
    {
        Result<bool, DbConnectionError> result = await _connectionTester.Run(SelectedProvider, ConnectionString);

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
    private void ConnectReceiver()
    {
        switch (SelectedReceiverType)
        {
            case ReceiverType.Tcp:
                StartTcp();
                break;
            case ReceiverType.Udp:
                StartUdp();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [RelayCommand]
    private async Task CreateDb() => await InitializeDatabaseAsync(connectAfterSuccess: true);

    [RelayCommand]
    private void DisconnectDb() => _storeService.Disconnect();

    [RelayCommand]
    private void DisconnectReceiver()
    {
        switch (SelectedReceiverType)
        {
            case ReceiverType.Tcp:
                StopTcp();
                break;
            case ReceiverType.Udp:
                StopUdp();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task InitializeDatabaseAsync(bool connectAfterSuccess)
    {
        DbInitializeStarted?.Invoke();

        try
        {
            await _storeService.InitializeAsync();
            await Task.Delay(2000);

            DbStatusType = await _storeService.GetDbStatusAsync();
            InitializeDbCommand.NotifyCanExecuteChanged();

            switch (connectAfterSuccess)
            {
                case true when DbStatusType == DbStatusType.NotInitialized:
                    DbInitializeCompleted?.Invoke(false, LocalizationService.Instance[Messages.ConnectionFailed]);
                    return;

                case true:
                    _storeService.Connect();
                    break;
            }

            DbInitializeCompleted?.Invoke(true, null);
        }
        catch (Exception exception)
        {
            DbInitializeCompleted?.Invoke(false, exception.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(CanInitializeDb))]
    private async Task InitializeDb() => await InitializeDatabaseAsync(connectAfterSuccess: false);

    private void NotifySaveStateChanged()
    {
        SaveCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(IsSaveEnabled));
    }

    partial void OnAutoConnectChanged(bool value) => NotifySaveStateChanged();

    partial void OnConnectionStringChanged(string value) => NotifySaveStateChanged();

    partial void OnHostChanged(string value) => NotifySaveStateChanged();

    partial void OnIsDbConnectedChanged(bool value) => InitializeDbCommand.NotifyCanExecuteChanged();

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

    partial void OnSelectedReceiverTypeChanged(ReceiverType value)
    {
        if (value == ReceiverType.Udp)
        {
            Host = string.Empty;
        }

        NotifySaveStateChanged();
    }

    private void OnStoreConnectionChanged(object? sender, EventArgs e) => IsDbConnected = _storeService.IsConnected;

    private void OnUdpListeningChanged(object? sender, bool isListening) =>
        Dispatcher.UIThread.Post(() => IsUdpListening = isListening);

    private async Task RefreshDbStatusAsync()
    {
        DbStatusType = await _storeService.GetDbStatusAsync();
        InitializeDbCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        _settings.DatabaseConnection.ConnectionString = ConnectionString;
        _settings.DatabaseConnection.Provider = SelectedProvider;
        _settings.Receiver.AutoConnect = AutoConnect;

        if (SelectedReceiverType == ReceiverType.Tcp)
        {
            _settings.Receiver.Host = Host;
        }

        _settings.Receiver.Port = int.Parse(Port);
        _settings.Receiver.Type = SelectedReceiverType;
        _settingsWriter.Save(_settings);

        NotifySaveStateChanged();
    }

    private void StartTcp() { }

    private void StartUdp()
    {
        if (!int.TryParse(Port, out int port))
        {
            return;
        }

        _dataProcessor.Start();

        _udpListenerService
            .Start(port)
            .Match(_ => { }, error => ConnectionTestCompleted?.Invoke(false, error.Value.Message));
    }

    private void StopTcp() { }

    private void StopUdp() => _udpListenerService.Stop();

    [RelayCommand]
    private async Task TestDbConnection()
    {
        Result<bool, DbConnectionError> result = await _connectionTester.Run(SelectedProvider, ConnectionString);

        result.Match(
            _ => ConnectionTestCompleted?.Invoke(true, null),
            error => ConnectionTestCompleted?.Invoke(false, error.Value.Message)
        );
    }

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
}
