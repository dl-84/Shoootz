using CommunityToolkit.Mvvm.ComponentModel;
using Sektionsliga.Services.Language;
using Sektionsliga.Services.Localization;
using Sektionsliga.Services.Settings;
using Sektionsliga.ViewModels.Competition;
using Sektionsliga.ViewModels.Info;
using Sektionsliga.ViewModels.Settings;

namespace Sektionsliga.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ISettingsService settingsService;

    private readonly ILanguageService languageService;

    private readonly ILocalizationService localizationService;

    [ObservableProperty]
    public partial ViewModelBase CurrentPage { get; set; }

    [ObservableProperty]
    public partial int ActiveIndex { get; set; } = 1;

    public MainWindowViewModel(
        ISettingsService settingsService,
        ILanguageService languageService,
        ILocalizationService localizationService
    )
    {
        this.settingsService = settingsService;
        this.languageService = languageService;
        this.localizationService = localizationService;

        CurrentPage = new EvaluationViewModel();
    }

    partial void OnActiveIndexChanged(int value)
    {
        CurrentPage = value switch
        {
            1 => new EvaluationViewModel(),
            3 => new GeneralViewModel(languageService, localizationService, settingsService),
            4 => new DatabaseViewModel(),
            5 => new GroupsViewModel(),
            7 => new VersionsViewModel(),
            _ => CurrentPage,
        };
    }
}
