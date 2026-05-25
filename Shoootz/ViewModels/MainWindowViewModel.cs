using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Shoootz.Models.Settings;
using Shoootz.Services.Database;
using Shoootz.Services.Language;
using Shoootz.Services.License;
using Shoootz.Services.Localization;
using Shoootz.Services.Settings;
using Shoootz.ViewModels.Competition;
using Shoootz.ViewModels.Error;
using Shoootz.ViewModels.Info;
using Shoootz.ViewModels.Settings;

namespace Shoootz.ViewModels;

internal partial class MainWindowViewModel : ViewModelBase
{
    private const int Index1EvaluateSite = 1;

    private const int Index3GeneralSite = 3;

    private const int Index4DatabaseSite = 4;

    private const int Index5GroupsSite = 5;

    private const int Index7AboutSite = 7;

    private const int Index8LicensesSite = 8;

    private readonly IDbConnectionTester _connectionTester;

    private readonly ILanguageService _languageService;

    private readonly ILicenseService _licenseService;

    private readonly ILocalizationService _localizationService;

    private readonly ISettingsService _settingsService;

    private SettingsModel? _settings;

    public MainWindowViewModel(
        IDbConnectionTester connectionTester,
        ILanguageService languageService,
        ILicenseService licenseService,
        ILocalizationService localizationService,
        ISettingsService settingsService
    )
    {
        _connectionTester = connectionTester;
        _languageService = languageService;
        _licenseService = licenseService;
        _localizationService = localizationService;
        _settingsService = settingsService;

        CurrentPage = new EvaluationViewModel();
    }

    [ObservableProperty]
    public partial int ActiveIndex { get; set; } = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNavbarEnabled))]
    public partial ViewModelBase CurrentPage { get; set; }

    [ObservableProperty]
    public partial bool IsDialogOpen { get; set; }

    public bool IsNavbarEnabled => CurrentPage is not SettingsErrorViewModel;

    public void InitSettings(SettingsModel settings)
    {
        _settings = settings;
    }

    public void RedirectToSettingsError(List<SettingsError> settingsErrors)
    {
        CurrentPage = new SettingsErrorViewModel(settingsErrors, _settingsService);
    }

    private DatabaseViewModel CreateDatabaseViewModel()
    {
        DatabaseViewModel viewModel = new DatabaseViewModel(
            _connectionTester,
            _settings ?? new SettingsModel(),
            _settingsService
        );
        viewModel.SettingsSaved += settings => _settings = settings;
        return viewModel;
    }

    private GeneralViewModel CreateGeneralViewModel()
    {
        GeneralViewModel viewModel = new GeneralViewModel(
            _languageService,
            _localizationService,
            _settings ?? new SettingsModel(),
            _settingsService
        );
        viewModel.SettingsSaved += settings => _settings = settings;
        return viewModel;
    }

    partial void OnActiveIndexChanged(int value)
    {
        ViewModelBase previous = CurrentPage;

        CurrentPage = value switch
        {
            Index1EvaluateSite => new EvaluationViewModel(),
            Index3GeneralSite => CreateGeneralViewModel(),
            Index4DatabaseSite => CreateDatabaseViewModel(),
            Index5GroupsSite => new GroupsViewModel(),
            Index7AboutSite => new AboutViewModel(_licenseService, _localizationService),
            Index8LicensesSite => new LicensesViewModel(_licenseService, _localizationService),
            _ => CurrentPage,
        };

        if (!ReferenceEquals(previous, CurrentPage) && previous is System.IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
