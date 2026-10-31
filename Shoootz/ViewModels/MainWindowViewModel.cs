using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Result;
using Shoootz.Common.Models.Language.Error;
using Shoootz.Common.Resources.Lang;
using Shoootz.Common.Services.Dialog;
using Shoootz.Common.Services.Localization;
using Shoootz.Common.Services.Settings;
using Shoootz.Common.ViewModels;
using Shoootz.Models.Database;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Incoming;
using Shoootz.Services.Receiver;
using Shoootz.Services.Store;
using Shoootz.Store;
using Shoootz.Store.Services;
using Shoootz.ViewModels.Competition;
using Shoootz.ViewModels.Info;
using Shoootz.ViewModels.Settings;
using Shoootz.Views.Types;

namespace Shoootz.ViewModels;

internal partial class MainWindowViewModel : ViewModelBase, IDialogHost
{
    private readonly IConnectionTester _connectionTester;

    private readonly ILocalizationService _localizationService;

    private readonly IServiceProvider _serviceProvider;

    private readonly SettingsModel _settings;

    private readonly ISettingsWriter<SettingsModel> _settingsWriter;

    private readonly IStoreService _storeService;

    private readonly IReceiverService _tcpListenerService;

    private readonly IReceiverService _udpListenerService;

    public MainWindowViewModel(
        IConnectionTester connectionTester,
        ILocalizationService localizationService,
        IOptionsMonitor<SettingsModel> settings,
        IServiceProvider serviceProvider,
        ISettingsWriter<SettingsModel> settingsWriter,
        IStoreService storeService,
        [FromKeyedServices(ReceiverType.Tcp)] IReceiverService tcpListenerService,
        [FromKeyedServices(ReceiverType.Udp)] IReceiverService udpListenerService
    )
    {
        _connectionTester = connectionTester;
        _localizationService = localizationService;
        _settings = settings.CurrentValue;
        _serviceProvider = serviceProvider;

        ConfiguredReceiverType = _settings.Receiver.Type;

        _settingsWriter = settingsWriter;
        _settingsWriter.SettingsSaved += settingsModel =>
        {
            ConfiguredReceiverType = settingsModel.Receiver.Type;
            _ = CheckDbConnectionAsync();
        };

        _tcpListenerService = tcpListenerService;
        _udpListenerService = udpListenerService;

        _tcpListenerService.IsListeningChanged += (_, isListening) =>
            Dispatcher.UIThread.Post(() => IsReceiverConnected = isListening);
        _udpListenerService.IsListeningChanged += (_, isListening) =>
            Dispatcher.UIThread.Post(() => IsReceiverConnected = isListening);

        _storeService = storeService;
        IsDbConnected = _storeService.IsConnected;
        _storeService.ConnectionChanged += (_, _) => IsDbConnected = _storeService.IsConnected;

        CurrentPage = CreateView(NavigationPageType.Evaluate)!;
        CurrentNavigationPageType = NavigationPageType.Evaluate;
    }

    public event Action? PendingMigrationsDetected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTcpReceiver))]
    public partial ReceiverType ConfiguredReceiverType { get; set; }

    [ObservableProperty]
    public partial NavigationPageType CurrentNavigationPageType { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNavbarEnabled))]
    public partial ViewModelBase CurrentPage { get; set; }

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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DbBadgeText))]
    public partial bool IsDbConnected { get; set; }

    [ObservableProperty]
    public partial bool IsDialogOpen { get; set; }

    public bool IsNavbarEnabled => CurrentPage is not Common.ViewModels.Error.SettingsErrorViewModel<SettingsModel>;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReceiverBadgeText))]
    public partial bool IsReceiverConnected { get; set; }

    public bool IsTcpReceiver => ConfiguredReceiverType == ReceiverType.Tcp;

    public string ReceiverBadgeText
    {
        get
        {
            string connectionType = _localizationService[nameof(Messages.UdpBroadcast)];

            string connectionState = _localizationService[
                IsReceiverConnected ? nameof(Messages.StatusConnected) : nameof(Messages.StatusDisconnected)
            ];

            return $"{connectionType} {connectionState}";
        }
    }

    public async Task CheckDbConnectionAsync()
    {
        Result<bool, DbConnectionError> result = await _connectionTester.Run(
            _settings.DatabaseConnection.Provider,
            _settings.DatabaseConnection.ConnectionString
        );

        result.Match(_ => { }, _ => _storeService.Disconnect());
    }

    public async Task CheckPendingMigrationsAsync()
    {
        DbStatusType statusType = await _storeService.GetDbStatusAsync();

        if (statusType == DbStatusType.UpdateAvailable)
        {
            NavigateTo(NavigationPageType.App);
            PendingMigrationsDetected?.Invoke();
        }
    }

    public void NavigateTo(NavigationPageType pageType)
    {
        ViewModelBase? next = CreateView(pageType);

        if (next is null)
        {
            return;
        }

        ViewModelBase previous = CurrentPage;
        CurrentPage = next;
        CurrentNavigationPageType = pageType;

        if (!ReferenceEquals(previous, CurrentPage) && previous is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public void RedirectToSettingsError(List<SettingsError> settingsErrors, string settings)
    {
        CurrentPage = new Common.ViewModels.Error.SettingsErrorViewModel<SettingsModel>(
            _settingsWriter,
            typeof(MainWindowViewModel).Assembly,
            "Shoootz.Resources.JsonSchemas.Settings.schema.json"
        )
        {
            EditorContent = settings,
            ErrorMessages = settingsErrors.Select(settingsError => settingsError.Message).ToList(),
        };
    }

    private ViewModelBase? CreateView(NavigationPageType pageType)
    {
        return pageType switch
        {
            NavigationPageType.About => _serviceProvider.GetRequiredService<AboutViewModel>(),
            NavigationPageType.App => _serviceProvider.GetRequiredService<ConnectionViewModel>(),
            NavigationPageType.Clubs => _serviceProvider.GetRequiredService<ClubsViewModel>(),
            NavigationPageType.Evaluate => _serviceProvider.GetRequiredService<EvaluationViewModel>(),
            NavigationPageType.Groups => _serviceProvider.GetRequiredService<GroupsViewModel>(),
            NavigationPageType.Licenses => _serviceProvider.GetRequiredService<LicensesViewModel>(),
            NavigationPageType.Logging => _serviceProvider.GetRequiredService<LoggingViewModel>(),
            NavigationPageType.Shooters => _serviceProvider.GetRequiredService<ShootersViewModel>(),
            NavigationPageType.Shots => _serviceProvider.GetRequiredService<ShotsViewModel>(),
            _ => null,
        };
    }
}
