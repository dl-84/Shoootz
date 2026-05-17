using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Sektionsliga.Models;
using Sektionsliga.Services.Settings;
using Sektionsliga.ViewModels.Settings.Dtos;

namespace Sektionsliga.ViewModels.Settings;

public partial class GeneralViewModel : ViewModelBase
{
    public static List<LanguageOptionDto> LanguageOptions => [new LanguageOptionDto("de"), new LanguageOptionDto("en")];

    [ObservableProperty]
    public partial LanguageOptionDto SelectedLanguageOption { get; set; }

    private bool _initialized;

    public GeneralViewModel()
    {
        AppSettings settings = SettingsService.Load();
        SelectedLanguageOption =
            LanguageOptions.FirstOrDefault(o => o.CultureInfo.TwoLetterISOLanguageName == settings.LanguageCultureCode)
            ?? LanguageOptions[0];
        _initialized = true;
    }

    partial void OnSelectedLanguageOptionChanged(LanguageOptionDto value)
    {
        if (!_initialized)
        {
            return;
        }
        SettingsService.Save(new AppSettings { LanguageCultureCode = value.CultureInfo.TwoLetterISOLanguageName });
    }
}
