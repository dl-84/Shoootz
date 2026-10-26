using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Shoootz.Bridge.TCP.Models.Settings;
using Shoootz.Bridge.TCP.Services.Bridge;
using Shoootz.Common.Services.Language;
using Shoootz.Common.Services.Localization;
using Shoootz.Common.Services.Settings;
using Shoootz.Common.ViewModels;
using Shoootz.Common.ViewModels.Settings;

namespace Shoootz.Bridge.TCP.ViewModels.Settings;

internal partial class AppViewModel : ViewModelBase, IDisposable, IHasUnsavedChanges
{
    private readonly SettingsModel _settings;

    private readonly ISettingsWriter<SettingsModel> _settingsWriter;

    public AppViewModel(
        IBridgeService bridgeService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IOptionsMonitor<SettingsModel> settings,
        ISettingsWriter<SettingsModel> settingsWriter
    )
    {
        _settings = settings.CurrentValue;
        _settingsWriter = settingsWriter;

        Bridge = bridgeService;
        Bridge.PropertyChanged += OnBridgePropertyChanged;

        AutoConnect = _settings.AutoConnect;

        GeneralSettings = new GeneralSettingsViewModel<SettingsModel>(
            languageService,
            localizationService,
            settings,
            settingsWriter
        );
    }

    [ObservableProperty]
    public partial bool AutoConnect { get; set; }

    public IBridgeService Bridge { get; }

    public GeneralSettingsViewModel<SettingsModel> GeneralSettings { get; }

    public bool IsSaveEnabled => CanSave();

    public void Dispose()
    {
        Bridge.PropertyChanged -= OnBridgePropertyChanged;
    }

    private bool CanSave() =>
        Bridge is { IsTcpPortValid: true, IsUdpPortValid: true }
        && (
            int.Parse(Bridge.TcpPort) != _settings.TcpPort
            || int.Parse(Bridge.UdpPort) != _settings.UdpPort
            || AutoConnect != _settings.AutoConnect
        );

    private void NotifySaveStateChanged()
    {
        SaveCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(IsSaveEnabled));
    }

    private void OnBridgePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(IBridgeService.TcpPort) or nameof(IBridgeService.UdpPort))
        {
            NotifySaveStateChanged();
        }
    }

    partial void OnAutoConnectChanged(bool value) => NotifySaveStateChanged();

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        _settings.AutoConnect = AutoConnect;
        _settings.TcpPort = int.Parse(Bridge.TcpPort);
        _settings.UdpPort = int.Parse(Bridge.UdpPort);
        _settingsWriter.Save(_settings);

        NotifySaveStateChanged();
    }
}
