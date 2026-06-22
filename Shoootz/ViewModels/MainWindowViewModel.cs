using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Result;
using Shoootz.Models.Database;
using Shoootz.Models.Error;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Database;
using Shoootz.Resources.Lang;
using Shoootz.Services.Localization;
using Shoootz.Services.Settings;
using Shoootz.Services.Store;
using Shoootz.Services.Udp;
using Shoootz.Store;
using Shoootz.Store.Services;
using Shoootz.ViewModels.Competition;
using Shoootz.ViewModels.Error;
using Shoootz.ViewModels.Info;
using Shoootz.ViewModels.Settings;

namespace Shoootz.ViewModels;

internal partial class MainWindowViewModel : ViewModelBase
{
    private readonly IConnectionTester _connectionTester;

    private readonly ILocalizationService _localizationService;

    private readonly IServiceProvider _serviceProvider;

    private readonly SettingsModel _settings;

    private readonly ISettingsWriter _settingsWriter;

    private readonly IUdpListenerService _udpListenerService;

    public MainWindowViewModel(
        IConnectionTester connectionTester,
        ILocalizationService localizationService,
        IOptionsMonitor<SettingsModel> settings,
        IServiceProvider serviceProvider,
        ISettingsWriter settingsWriter,
        IUdpListenerService udpListenerService
    )
    {
        _connectionTester = connectionTester;
        _localizationService = localizationService;
        _settings = settings.CurrentValue;
        _serviceProvider = serviceProvider;

        _settingsWriter = settingsWriter;
        _settingsWriter.SettingsSaved += _ => CheckDbConnection();

        _udpListenerService = udpListenerService;
        _udpListenerService.IsListeningChanged += (_, isListening) => IsUdpConnected = isListening;

        CurrentPage = CreateView(ActiveIndex)!;
    }

    public event Action? PendingMigrationsDetected;

    [ObservableProperty]
    public partial int ActiveIndex { get; set; } = 1;

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

    public bool IsNavbarEnabled => CurrentPage is not SettingsErrorViewModel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UdpBadgeText))]
    public partial bool IsUdpConnected { get; set; }

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
        Result<bool, DbConnectionError> result = _settings.DatabaseConnection.Provider switch
        {
            ProviderType.PostgreSql => _connectionTester.PostgreSql(_settings.DatabaseConnection.ConnectionString),
            _ => _connectionTester.Sqlite(_settings.DatabaseConnection.ConnectionString),
        };

        result.Match(_ => IsDbConnected = true, _ => IsDbConnected = false);
    }

    public async Task CheckPendingMigrationsAsync()
    {
        IStoreService? storeService = _serviceProvider.GetService<IStoreService>();
        if (storeService is null)
        {
            return;
        }

        DbStatus status = await storeService.GetDbStatusAsync();

        if (status == DbStatus.UpdateAvailable)
        {
            ActiveIndex = 4;
            PendingMigrationsDetected?.Invoke();
        }
    }

    public void RedirectToSettingsError(List<SettingsError> settingsErrors, string settings)
    {
        CurrentPage = new SettingsErrorViewModel(_settingsWriter);

        (CurrentPage as SettingsErrorViewModel)?.ErrorMessages = settingsErrors
            .Select(settingsError => settingsError.Message)
            .ToList();

        (CurrentPage as SettingsErrorViewModel)?.EditorContent = settings;
    }

    private ViewModelBase? CreateView(int index)
    {
        return index switch
        {
            1 => _serviceProvider.GetRequiredService<EvaluationViewModel>(),
            3 => _serviceProvider.GetRequiredService<GeneralViewModel>(),
            4 => _serviceProvider.GetRequiredService<ConnectionViewModel>(),
            5 => _serviceProvider.GetRequiredService<GroupsViewModel>(),
            7 => _serviceProvider.GetRequiredService<AboutViewModel>(),
            8 => _serviceProvider.GetRequiredService<LicensesViewModel>(),
            _ => null,
        };
    }

    partial void OnActiveIndexChanged(int value)
    {
        ViewModelBase previous = CurrentPage;
        ViewModelBase? next = CreateView(value);

        if (next is null)
        {
            return;
        }

        CurrentPage = next;

        if (!ReferenceEquals(previous, CurrentPage) && previous is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
