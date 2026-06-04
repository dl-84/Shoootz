using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Result;
using Shoootz.Factory.ViewModel;
using Shoootz.Models.Error;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Database;
using Shoootz.Resources.Lang;
using Shoootz.Services.Localization;
using Shoootz.Services.Settings;
using Shoootz.Services.Udp;
using Shoootz.Store;
using Shoootz.Store.Services;
using Shoootz.ViewModels.Error;

namespace Shoootz.ViewModels;

internal partial class MainWindowViewModel : ViewModelBase
{
    private readonly IConnectionTester _connectionTester;

    private readonly ILocalizationService _localizationService;

    private readonly ISettingsService _settingsService;

    private readonly IUdpListenerService _udpListenerService;

    private readonly IViewModelFactory _viewModelFactory;

    public MainWindowViewModel(
        IConnectionTester connectionTester,
        ILocalizationService localizationService,
        ISettingsService settingsService,
        IUdpListenerService udpListenerService,
        IViewModelFactory viewModelFactory
    )
    {
        _connectionTester = connectionTester;
        _localizationService = localizationService;

        _settingsService = settingsService;
        _settingsService.SettingsSaved += _ => CheckDbConnection();

        _udpListenerService = udpListenerService;
        _udpListenerService.IsListeningChanged += (_, isListening) => IsUdpConnected = isListening;

        _viewModelFactory = viewModelFactory;
        CurrentPage = _viewModelFactory.CreateView(ActiveIndex)!;
    }

    [ObservableProperty]
    public partial int ActiveIndex { get; set; } = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNavbarEnabled))]
    public partial ViewModelBase CurrentPage { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DbBadgeText))]
    public partial bool IsDbConnected { get; set; }

    [ObservableProperty]
    public partial bool IsDialogOpen { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UdpBadgeText))]
    public partial bool IsUdpConnected { get; set; }

    public bool IsNavbarEnabled => CurrentPage is not SettingsErrorViewModel;

    public string DbBadgeText
    {
        get
        {
            string connectionType = _localizationService[nameof(Messages.Database)];

            string connectionState = _localizationService[
                IsDbConnected ? nameof(Messages.StatusConnected) : nameof(Messages.StatusDisconnected)
            ];
            return $"{connectionType} {connectionState}";
        }
    }

    public string UdpBadgeText
    {
        get
        {
            string connectionType = _localizationService[nameof(Messages.Udp)];

            string connectionState = _localizationService[
                IsUdpConnected ? nameof(Messages.StatusConnected) : nameof(Messages.StatusDisconnected)
            ];

            return $"{connectionType} {connectionState}";
        }
    }

    public void CheckDbConnection()
    {
        SettingsModel? settings = _settingsService.CurrentSettings;

        if (settings is null)
        {
            return;
        }

        Result<bool, DbConnectionError> result = settings.DbConnectionModel.ProviderType switch
        {
            ProviderType.PostgreSql => _connectionTester.PostgreSql(settings.DbConnectionModel.ConnectionString),
            _ => _connectionTester.Sqlite(settings.DbConnectionModel.ConnectionString),
        };

        result.Match(_ => IsDbConnected = true, _ => IsDbConnected = false);
    }

    public void RedirectToSettingsError(List<SettingsError> settingsErrors)
    {
        CurrentPage = new SettingsErrorViewModel(settingsErrors, _settingsService);
    }

    partial void OnActiveIndexChanged(int value)
    {
        ViewModelBase previous = CurrentPage;
        ViewModelBase? next = _viewModelFactory.CreateView(value);

        if (next is null)
        {
            return;
        }

        CurrentPage = next;

        if (!ReferenceEquals(previous, CurrentPage) && previous is System.IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
