using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Language;
using Shoootz.Services.App;
using Shoootz.Services.Grafik;
using Shoootz.Services.Language;
using Shoootz.Services.Localization;
using Shoootz.Services.Settings;

namespace Shoootz.ViewModels.Settings;

internal partial class GeneralViewModel : ViewModelBase
{
    private readonly IGrafikService _grafikService;

    private bool _allowSave;

    private readonly ILocalizationService _localizationService;

    private readonly List<SettingsError>? _settingsErrors;

    private readonly ISettingsService _settingsService;

    private readonly SettingsModel _settings;

    public GeneralViewModel(
        IGrafikService grafikService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        SettingsModel settings,
        List<SettingsError>? settingsErrors,
        ISettingsService settingsService
    )
    {
        LanguageOptions = languageService.GetAvailableLanguages();

        _grafikService = grafikService;
        _localizationService = localizationService;
        _settings = settings;
        _settingsErrors = settingsErrors;
        _settingsService = settingsService;

        SelectedLanguageOption = HasCurrentLanguageCodeError ? null : GetLanguage(settings.CurrentLanguageCode);
        _allowSave = true;
    }

    public event Action? DeleteSettingsFolderRequested;

    public event Action? DeleteSettingsFileRequested;

    public event Action<string>? SettingsContentRequested;

    public event Action? SettingsErrorsChanged;

    public event Action<SettingsModel>? SettingsSaved;

    public IEnumerable<string> CriticalErrorMessages
    {
        get
        {
            return _settingsErrors
                    ?.Where(settingsError =>
                        settingsError.PropertyType
                            is SettingsPropertyType.ExceptionOnReadContent
                                or SettingsPropertyType.JsonExceptionOnValidate
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
                    settingsError.PropertyType
                        is SettingsPropertyType.ExceptionOnReadContent
                            or SettingsPropertyType.JsonExceptionOnValidate
                ) ?? false;
        }
    }

    public bool HasCurrentLanguageCodeError
    {
        get
        {
            return _settingsErrors is not null
                && _settingsErrors.Count > 0
                && _settingsErrors.Any(settingsError =>
                    settingsError.PropertyType is SettingsPropertyType.CurrentLanguageCode
                );
        }
    }

    public bool HasValidationErrors => _settingsErrors is not null && _settingsErrors.Count > 0;

    public string? InvalidLanguageCodeValue =>
        _settingsErrors
            ?.FirstOrDefault(settingsError => settingsError.PropertyType is SettingsPropertyType.CurrentLanguageCode)
            ?.Value;

    public List<LanguageOptionModel> LanguageOptions { get; }

    [ObservableProperty]
    public partial LanguageOptionModel? SelectedLanguageOption { get; set; }

    public void ExecuteDeleteSettingsFile()
    {
        _settingsService.DeleteSettingsFile();
    }

    public void ExecuteDeleteSettingsFolder()
    {
        _settingsService.DeleteSettingsFolder();
    }

    [RelayCommand]
    private void DeleteSettingsFolder()
    {
        DeleteSettingsFolderRequested?.Invoke();
    }

    [RelayCommand]
    private void DeleteSettingsFile()
    {
        DeleteSettingsFileRequested?.Invoke();
    }

    [RelayCommand]
    private void ShowSettingsContent()
    {
        SettingsContentRequested?.Invoke(_settingsService.LoadRaw());
    }

    private LanguageOptionModel GetLanguage(string cultureCode)
    {
        return LanguageOptions.FirstOrDefault(languageModel =>
                languageModel.CultureInfo.TwoLetterISOLanguageName == cultureCode
            ) ?? LanguageOptions[0];
    }

    partial void OnSelectedLanguageOptionChanged(LanguageOptionModel? value)
    {
        if (value is null || !_allowSave)
        {
            return;
        }

        _settings.CurrentLanguageCode = value.CultureInfo.TwoLetterISOLanguageName;
        _settingsService.Save(_settings);
        SettingsSaved?.Invoke(_settings);
        _localizationService.SetLanguage(value.CultureInfo.TwoLetterISOLanguageName);

        _settingsErrors?.RemoveAll(e => e.PropertyType is SettingsPropertyType.CurrentLanguageCode);

        OnPropertyChanged(nameof(HasCurrentLanguageCodeError));
        OnPropertyChanged(nameof(HasValidationErrors));
        OnPropertyChanged(nameof(InvalidLanguageCodeValue));

        SettingsErrorsChanged?.Invoke();
    }

    [RelayCommand]
    private void OpenSettingsFolder()
    {
        Process.Start(new ProcessStartInfo { FileName = AppPath.AppDataBase, UseShellExecute = true });
    }
}
