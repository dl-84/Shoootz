using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Shoootz.Common.Models.Language;
using Shoootz.Common.Services.App;
using Shoootz.Common.Services.Language;
using Shoootz.Common.Services.Localization;
using Shoootz.Common.Services.Settings;

namespace Shoootz.Common.ViewModels.Settings;

/// <inheritdoc/>
public partial class GeneralSettingsViewModel<TSettings> : GeneralSettingsViewModelBase
    where TSettings : ILanguageSettings
{
    private readonly Func<Task<string?>>? _additionalDeleteFolderCleanup;

    private readonly ILocalizationService _localizationService;

    private readonly ISettingsWriter<TSettings> _settingsWriter;

    private TSettings _settings;

    public GeneralSettingsViewModel(
        ILanguageService languageService,
        ILocalizationService localizationService,
        IOptionsMonitor<TSettings> settings,
        ISettingsWriter<TSettings> settingsWriter,
        Func<Task<string?>>? additionalDeleteFolderCleanup = null
    )
    {
        _additionalDeleteFolderCleanup = additionalDeleteFolderCleanup;
        _localizationService = localizationService;
        _settings = settings.CurrentValue;
        _settingsWriter = settingsWriter;

        LanguageOptions = languageService.GetAvailableLanguages();
        SelectedLanguageOption = GetLanguage(_settings.CurrentLanguageCode);
    }

    public override bool HasAdditionalDeleteFolderCleanup => _additionalDeleteFolderCleanup is not null;

    public List<LanguageOptionModel> LanguageOptions { get; }

    [ObservableProperty]
    public partial LanguageOptionModel? SelectedLanguageOption { get; set; }

    public override void ExecuteDeleteSettingsFile()
    {
        _settingsWriter.DeleteSettingsFile();
    }

    public override async Task ExecuteDeleteSettingsFolderAsync()
    {
        RaiseDeleteAllStarted();

        string? error = _additionalDeleteFolderCleanup is null ? null : await _additionalDeleteFolderCleanup();

        if (error is not null)
        {
            RaiseDeleteAllCompleted(false, error);
            return;
        }

        _settingsWriter.DeleteSettingsFolder();
        RaiseDeleteAllCompleted(true, null);
    }

    [RelayCommand]
    private static void OpenSettingsFolder()
    {
        Process.Start(new ProcessStartInfo { FileName = AppPath.AppDataBase, UseShellExecute = true });
    }

    [RelayCommand]
    private void DeleteSettingsFile()
    {
        RaiseDeleteSettingsFileRequested();
    }

    [RelayCommand]
    private void DeleteSettingsFolder()
    {
        RaiseDeleteSettingsFolderRequested();
    }

    private LanguageOptionModel GetLanguage(string cultureCode)
    {
        return LanguageOptions.FirstOrDefault(languageOption =>
                languageOption.CultureInfo.TwoLetterISOLanguageName == cultureCode
            ) ?? LanguageOptions[0];
    }

    partial void OnSelectedLanguageOptionChanged(LanguageOptionModel? value)
    {
        if (value is null || value.CultureInfo.TwoLetterISOLanguageName == _settings.CurrentLanguageCode)
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
            RaiseSettingsContentRequested,
            error => RaiseReadSettingsFileFailed(error.Value.Message)
        );
    }
}
