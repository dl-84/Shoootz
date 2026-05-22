using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Shoootz.Models;
using Shoootz.Services.Grafik;
using Shoootz.Services.Language;
using Shoootz.Services.License;
using Shoootz.Services.Localization;
using Shoootz.Services.Settings;
using Shoootz.ViewModels.Competition;
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

    private readonly IGrafikService _grafikService;

    private readonly ILanguageService _languageService;

    private readonly ILocalizationService _localizationService;

    private readonly ISettingsService _settingsService;

    private readonly ILicenseService _licenseService;

    private SettingsModel? _settings;

    private List<SettingsError>? _settingsErrors;

    public MainWindowViewModel(
        IGrafikService grafikService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        ISettingsService settingsService,
        ILicenseService licenseService
    )
    {
        _grafikService = grafikService;
        _languageService = languageService;
        _localizationService = localizationService;
        _settingsService = settingsService;
        _licenseService = licenseService;

        CurrentPage = new EvaluationViewModel();
    }

    [ObservableProperty]
    public partial int ActiveIndex { get; set; } = 1;

    [ObservableProperty]
    public partial ViewModelBase CurrentPage { get; set; }

    public bool HasSettingsErrors => _settingsErrors is not null && _settingsErrors.Count > 0;

    [ObservableProperty]
    public partial bool IsDialogOpen { get; set; }

    public void InitSettings(SettingsModel settings)
    {
        _settings = settings;
    }

    public void RedirectToSettings(List<SettingsError> settingsErrors)
    {
        _settingsErrors = settingsErrors;
        ActiveIndex = Index3GeneralSite;
    }

    private GeneralViewModel CreateGeneralViewModel()
    {
        GeneralViewModel viewModel = new GeneralViewModel(
            _grafikService,
            _languageService,
            _localizationService,
            _settingsService,
            _settings,
            _settingsErrors
        );

        viewModel.SettingsErrorsChanged += () => OnPropertyChanged(nameof(HasSettingsErrors));

        return viewModel;
    }

    partial void OnActiveIndexChanged(int value)
    {
        ViewModelBase previous = CurrentPage;

        CurrentPage = value switch
        {
            Index1EvaluateSite => new EvaluationViewModel(),
            Index3GeneralSite => CreateGeneralViewModel(),
            Index4DatabaseSite => new DatabaseViewModel(),
            Index5GroupsSite => new GroupsViewModel(),
            Index7AboutSite => new AboutViewModel(_licenseService, _localizationService),
            Index8LicensesSite => new LicensesViewModel(_licenseService, _localizationService),
            _ => CurrentPage,
        };

        if (!ReferenceEquals(previous, CurrentPage) && previous is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
