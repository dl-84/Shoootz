using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektionsliga.ViewModels.Settings;

public enum DisplayLanguage
{
    German,
    English,
}

public record LanguageOption(DisplayLanguage Value, string DisplayName);

public partial class GeneralViewModel : ViewModelBase
{
    public List<LanguageOption> LanguageOptions { get; } =
    [new(DisplayLanguage.German, "Deutsch"), new(DisplayLanguage.English, "English")];

    [ObservableProperty]
    public partial LanguageOption SelectedLanguageOption { get; set; }

    public GeneralViewModel()
    {
        SelectedLanguageOption = LanguageOptions[0];
    }
}
