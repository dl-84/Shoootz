using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Sektionsliga.Models;
using Sektionsliga.Services.Flag;
using Sektionsliga.Services.Settings;

namespace Sektionsliga.ViewModels.Settings;

public partial class GeneralViewModel : ViewModelBase
{
    private readonly ISettingsService settingsService;

    public List<LanguageOptionModel> LanguageOptions { get; }

    [ObservableProperty]
    public partial LanguageOptionModel SelectedLanguageOption { get; set; }

    public GeneralViewModel(ISettingsService settingsService, IFlagService flagService)
    {
        this.settingsService = settingsService;
        LanguageOptions =
        [
            new LanguageOptionModel("de", flagService.GetFlag("de")),
            new LanguageOptionModel("en", flagService.GetFlag("en")),
        ];

        AppSettingsModel settings = this.settingsService.Load();

        SelectedLanguageOption =
            LanguageOptions.FirstOrDefault(o => o.CultureInfo.TwoLetterISOLanguageName == settings.LanguageCultureCode)
            ?? LanguageOptions[0];
    }

    partial void OnSelectedLanguageOptionChanged(LanguageOptionModel value)
    {
        settingsService.Save(new AppSettingsModel { LanguageCultureCode = value.CultureInfo.TwoLetterISOLanguageName });
    }
}
