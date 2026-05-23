using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Database;
using Shoootz.Services.Settings;

namespace Shoootz.ViewModels.Settings;

internal partial class DatabaseViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;

    private readonly SettingsModel _settings;

    private readonly List<SettingsError>? _settingsErrors;

    public DatabaseViewModel(
        SettingsModel settings,
        List<SettingsError>? settingsErrors,
        ISettingsService settingsService
    )
    {
        _settings = settings;
        _settingsErrors = settingsErrors;
        _settingsService = settingsService;

        SelectedProvider = settings.DbConnectionModel.ProviderType;
        ConnectionString = settings.DbConnectionModel.ConnectionString;
    }

    public event Action<SettingsModel>? SettingsSaved;

    public bool HasValidationErrors => _settingsErrors is not null && _settingsErrors.Count > 0;

    public IEnumerable<ProviderType> ProviderOptions { get; } = Enum.GetValues<ProviderType>();

    [ObservableProperty]
    public partial string ConnectionString { get; set; }

    [ObservableProperty]
    public partial ProviderType SelectedProvider { get; set; }

    [RelayCommand]
    private void Save()
    {
        _settings.DbConnectionModel.ConnectionString = ConnectionString;
        _settings.DbConnectionModel.ProviderType = SelectedProvider;
        _settingsService.Save(_settings);
        SettingsSaved?.Invoke(_settings);
    }
}
