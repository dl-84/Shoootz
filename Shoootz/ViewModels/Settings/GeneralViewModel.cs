using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shoootz.Models;
using Shoootz.Services.Grafik;
using Shoootz.Services.Language;
using Shoootz.Services.Localization;
using Shoootz.Services.Settings;

namespace Shoootz.ViewModels.Settings;

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

        SelectedLanguageOption = HasCurrentLanguageCodeError
            ? null
            : GetLanguage(settings?.CurrentLanguageCode ?? "en");
    }

    public event Action? SettingsErrorsChanged;

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

    public bool HasCurrentLanguageCodeError
    {
        get
        {
            return _settingsErrors is not null
                && _settingsErrors.Count > 0
                && _settingsErrors.Any(settingsError => settingsError.Property is SettingsProperty.CurrentLanguageCode);
        }
    }

    public bool HasValidationErrors => _settingsErrors is not null && _settingsErrors.Count > 0;

    public string? InvalidLanguageCodeValue =>
        _settingsErrors
            ?.FirstOrDefault(settingsError => settingsError.Property is SettingsProperty.CurrentLanguageCode)
            ?.Value;

    public List<LanguageOptionModel> LanguageOptions { get; }

    [ObservableProperty]
    public partial LanguageOptionModel? SelectedLanguageOption { get; set; }

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

    partial void OnSelectedLanguageOptionChanged(LanguageOptionModel? value)
    {
        if (value is null)
        {
            return;
        }

        SettingsModel settingsModel = _settings ?? new SettingsModel();
        settingsModel.CurrentLanguageCode = value.CultureInfo.TwoLetterISOLanguageName;
        _settingsService.Save(settingsModel);
        _localizationService.SetLanguage(value.CultureInfo.TwoLetterISOLanguageName);

        _settingsErrors?.RemoveAll(e => e.Property is SettingsProperty.CurrentLanguageCode);

        OnPropertyChanged(nameof(HasCurrentLanguageCodeError));
        OnPropertyChanged(nameof(HasValidationErrors));
        OnPropertyChanged(nameof(InvalidLanguageCodeValue));

        SettingsErrorsChanged?.Invoke();
    }

    [RelayCommand]
    private void OpenSettingsFolder()
    {
        Process.Start(new ProcessStartInfo { FileName = _settingsService.FolderPath, UseShellExecute = true });
    }
}
