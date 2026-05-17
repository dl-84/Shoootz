using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Sektionsliga.ViewModels.Settings.Dtos;

namespace Sektionsliga.ViewModels.Settings;

public partial class GeneralViewModel : ViewModelBase
{
    public static List<LanguageOptionDto> LanguageOptions => [new LanguageOptionDto("de"), new LanguageOptionDto("en")];

    [ObservableProperty]
    public partial LanguageOptionDto SelectedLanguageOption { get; set; }

    public GeneralViewModel()
    {
        SelectedLanguageOption = LanguageOptions[0];
    }
}
