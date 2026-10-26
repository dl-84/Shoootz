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
                Result<Unit, StoreSaveError> result = await _storeService.DeleteDatabaseAsync();

                return result.Match(
                    _ => null!,
                    error =>
                        $"{LocalizationService.Instance[Messages.DeleteDatabaseFailedHint]}\n\n{error.Value.Message}"
                );
            }
        );

        AutoConnect = _settings.UdpConnection.AutoConnect;
        IsUdpListening = udpListenerService.IsListening;
        Port = _settings.UdpConnection.Port.ToString();
        SelectedProvider = _settings.DatabaseConnection.Provider;
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

    [ObservableProperty]
    public partial string ConnectionString { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InitializeDbLabel))]
    [NotifyPropertyChangedFor(nameof(IsInitializeDbEnabled))]
    public partial DbStatus DbStatus { get; set; }

    public GeneralSettingsViewModel<SettingsModel> GeneralSettings { get; }

    public string InitializeDbLabel =>
        !IsDbConnected
            ? LocalizationService.Instance[Messages.DbNotConnected]
            : DbStatus switch
            {
                DbStatus.UpdateAvailable => LocalizationService.Instance[Messages.UpdateDb],
                DbStatus.UpToDate => LocalizationService.Instance[Messages.DbUpToDate],
                _ => LocalizationService.Instance[Messages.InitializeDb],
            };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InitializeDbLabel))]
    [NotifyPropertyChangedFor(nameof(IsInitializeDbEnabled))]
    public partial bool IsDbConnected { get; set; }

    public bool IsInitializeDbEnabled => DbStatus != DbStatus.UpToDate && IsDbConnected;

    public bool IsPortValid => Port.IsValidPort();

    public bool IsSaveEnabled => CanSave();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UdpStatusLabel))]
    public partial bool IsUdpListening { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPortValid))]
    public partial string Port { get; set; }

    public IEnumerable<ProviderType> ProviderOptions { get; } = Enum.GetValues<ProviderType>();

    [ObservableProperty]
    public partial ProviderType SelectedProvider { get; set; }

    public string UdpStatusLabel =>
        LocalizationService.Instance[IsUdpListening ? nameof(Messages.Connected) : nameof(Messages.NotConnected)];

    public void Dispose()
    {
        _udpListenerService.IsListeningChanged -= OnUdpListeningChanged;
        _storeService.ConnectionChanged -= OnStoreConnectionChanged;
    }

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
        Port.IsValidPort()
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

    private void OnStoreConnectionChanged(object? sender, EventArgs e) => IsDbConnected = _storeService.IsConnected;

    private void OnUdpListeningChanged(object? sender, bool isListening) =>
        Dispatcher.UIThread.Post(() => IsUdpListening = isListening);

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
    private async Task CreateDb() => await InitializeDatabaseAsync(connectAfterSuccess: true);

    private async Task InitializeDatabaseAsync(bool connectAfterSuccess)
    {
        DbInitializeStarted?.Invoke();

        try
        {
            await _storeService.InitializeAsync();
            await Task.Delay(2000);

            DbStatus = await _storeService.GetDbStatusAsync();
            InitializeDbCommand.NotifyCanExecuteChanged();

            switch (connectAfterSuccess)
            {
                case true when DbStatus == DbStatus.NotInitialized:
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

    [RelayCommand]
    private void DisconnectDb() => _storeService.Disconnect();

    [RelayCommand]
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
    private async Task InitializeDb() => await InitializeDatabaseAsync(connectAfterSuccess: false);

    private bool CanInitializeDb() => IsInitializeDbEnabled;

    private async Task RefreshDbStatusAsync()
    {
        DbStatus = await _storeService.GetDbStatusAsync();
        InitializeDbCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task TestDbConnection()
    {
        Result<bool, DbConnectionError> result = await _connectionTester.Run(SelectedProvider, ConnectionString);

        result.Match(
            _ => ConnectionTestCompleted?.Invoke(true, null),
            error => ConnectionTestCompleted?.Invoke(false, error.Value.Message)
        );
    }
}
