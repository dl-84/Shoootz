using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Sektionsliga.Models;
using Sektionsliga.Services.Grafik;
using Sektionsliga.Services.Language;
using Sektionsliga.Services.Localization;
using Sektionsliga.Services.Settings;
using Sektionsliga.ViewModels.Competition;
using Sektionsliga.ViewModels.Info;
using Sektionsliga.ViewModels.Settings;

namespace Sektionsliga.ViewModels;

internal partial class MainWindowViewModel : ViewModelBase
{
    private readonly IGrafikService _grafikService;

    private readonly ILanguageService _languageService;

    private readonly ILocalizationService _localizationService;

    private readonly ISettingsService _settingsService;

    private SettingsModel? _settings;

    private List<SettingsError>? _settingsErrors;

    public MainWindowViewModel(
        IGrafikService grafikService,
        ISettingsService settingsService,
        ILanguageService languageService,
        ILocalizationService localizationService
    )
    {
        _grafikService = grafikService;
        _settingsService = settingsService;
        _languageService = languageService;
        _localizationService = localizationService;

        CurrentPage = new EvaluationViewModel();
    }

    [ObservableProperty]
    public partial int ActiveIndex { get; set; } = 1;

    [ObservableProperty]
    public partial ViewModelBase CurrentPage { get; set; }

    public bool HasSettingsErrors => _settingsErrors is not null && _settingsErrors.Count > 0;

    public void InitSettings(SettingsModel settings)
    {
        _settings = settings;
    }

    public void RedirectToSettings(List<SettingsError> settingsErrors)
    {
        _settingsErrors = settingsErrors;
        ActiveIndex = 3;
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
        CurrentPage = value switch
        {
            1 => new EvaluationViewModel(),
            3 => CreateGeneralViewModel(),
            4 => new DatabaseViewModel(),
            5 => new GroupsViewModel(),
            7 => new VersionsViewModel(),
            _ => CurrentPage,
        };
    }
}
