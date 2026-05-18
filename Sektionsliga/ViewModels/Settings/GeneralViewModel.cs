using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektionsliga.Models;
using Sektionsliga.Services.Grafik;
using Sektionsliga.Services.Language;
using Sektionsliga.Services.Localization;
using Sektionsliga.Services.Settings;

namespace Sektionsliga.ViewModels.Settings;

internal partial class GeneralViewModel : ViewModelBase
{
    private readonly IGrafikService _grafikService;

    private readonly ILocalizationService _localizationService;

    private readonly SettingsModel? _settings;

    private readonly List<SettingsError>? _settingsErrors;

    private readonly ISettingsService _settingsService;

    public GeneralViewModel(
        IGrafikService grafikService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        ISettingsService settingsService,
        SettingsModel? settings,
        List<SettingsError>? settingsErrors
    )
    {
        LanguageOptions = languageService.GetAvailableLanguages();

        _grafikService = grafikService;
        _localizationService = localizationService;
        _settingsService = settingsService;
        _settings = settings;
        _settingsErrors = settingsErrors;

        SelectedLanguageOption = GetLanguage(settings?.CurrentLanguageCode ?? "unknown");
    }

    public IEnumerable<string> CriticalErrorMessages
    {
        get
        {
            return _settingsErrors
                    ?.Where(settingsError =>
                        settingsError.Property
                            is SettingsProperty.ExceptionOnReadContent
                                or SettingsProperty.JsonExceptionOnValidate
                    )
                    .Select(settingsError => settingsError.Message)
                ?? [];
        }
    }

    public Bitmap ErrorTriangle => _grafikService.GetErrorTriangle;

    public bool HasCriticalError
    {
        get
        {
            return _settingsErrors?.Exists(settingsError =>
                    settingsError.Property
                        is SettingsProperty.ExceptionOnReadContent
                            or SettingsProperty.JsonExceptionOnValidate
                ) ?? false;
        }
    }

    public string? InvalidLanguageCodeValue =>
        _settingsErrors
            ?.FirstOrDefault(settingsError => settingsError.Property is SettingsProperty.CurrentLanguageCode)
            ?.Value;

    public bool HasCurrentLanguageCodeError
    {
        get
        {
            return _settingsErrors is not null
                && _settingsErrors.Count > 0
                && _settingsErrors.Any(settingsError => settingsError.Property is SettingsProperty.CurrentLanguageCode);
        }
    }

    public List<LanguageOptionModel> LanguageOptions { get; }

    [ObservableProperty]
    public partial LanguageOptionModel SelectedLanguageOption { get; set; }

    [RelayCommand]
    private void DeleteSettings()
    {
        _settingsService.Delete();
    }

    private LanguageOptionModel GetLanguage(string cultureCode)
    {
        return LanguageOptions.FirstOrDefault(languageModel =>
                languageModel.CultureInfo.TwoLetterISOLanguageName == cultureCode
            ) ?? LanguageOptions[0];
    }

    partial void OnSelectedLanguageOptionChanged(LanguageOptionModel value)
    {
        _settings?.CurrentLanguageCode = value.CultureInfo.TwoLetterISOLanguageName;
        _settingsService.Save(_settings);
        _localizationService.SetLanguage(value.CultureInfo.TwoLetterISOLanguageName);
    }

    [RelayCommand]
    private void OpenSettingsFolder()
    {
        Process.Start(new ProcessStartInfo { FileName = _settingsService.FolderPath, UseShellExecute = true });
    }
}
