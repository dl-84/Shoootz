using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Language;
using Shoootz.Services.App;
using Shoootz.Services.Language;
using Shoootz.Services.Localization;
using Shoootz.Services.Settings;
using Shoootz.Services.Settings.Read;

namespace Shoootz.ViewModels.Settings;

internal partial class GeneralViewModel : ViewModelBase
{
    private readonly bool _allowSave;

    private readonly ILocalizationService _localizationService;

    private readonly SettingsModel _settings;

    private readonly ISettingsWriter _settingsWriter;

    public GeneralViewModel(
        ILanguageService languageService,
        ILocalizationService localizationService,
        IOptions<SettingsModel> settings,
        ISettingsWriter settingsWriter
    )
    {
        LanguageOptions = languageService.GetAvailableLanguages();

        _localizationService = localizationService;
        _settings = settings.Value;
        _settingsWriter = settingsWriter;

        SelectedLanguageOption = GetLanguage(_settings.CurrentLanguageCode);
        _allowSave = true;
    }

    public event Action? DeleteSettingsFolderRequested;

    public event Action? DeleteSettingsFileRequested;

    public event Action<string>? ReadSettingsFileFailed;

    public event Action<string>? SettingsContentRequested;

    public List<LanguageOptionModel> LanguageOptions { get; }

    [ObservableProperty]
    public partial LanguageOptionModel? SelectedLanguageOption { get; set; }

    public void ExecuteDeleteSettingsFile()
    {
        _settingsWriter.DeleteSettingsFile();
    }

    public void ExecuteDeleteSettingsFolder()
    {
        _settingsWriter.DeleteSettingsFolder();
    }

    [RelayCommand]
    private static void OpenSettingsFolder()
    {
        Process.Start(new ProcessStartInfo { FileName = AppPath.AppDataBase, UseShellExecute = true });
    }

    [RelayCommand]
    private void DeleteSettingsFile()
    {
        DeleteSettingsFileRequested?.Invoke();
    }

    [RelayCommand]
    private void DeleteSettingsFolder()
    {
        DeleteSettingsFolderRequested?.Invoke();
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
        _settingsWriter.Save(_settings);
        _localizationService.SetLanguage(value.CultureInfo.TwoLetterISOLanguageName);
    }

    [RelayCommand]
    private void ShowSettingsContent()
    {
        SettingsReader.Read.Match(
            value => SettingsContentRequested?.Invoke(value),
            error => ReadSettingsFileFailed?.Invoke(error.Value.Message)
        );
    }
}
