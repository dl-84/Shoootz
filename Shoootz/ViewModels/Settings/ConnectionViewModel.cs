using System;
using System.Collections.Generic;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Database;
using Shoootz.Services.App;
using Shoootz.Services.Database;
using Shoootz.Services.Grafik;
using Shoootz.Services.Settings;

namespace Shoootz.ViewModels.Settings;

internal partial class ConnectionViewModel : ViewModelBase
{
    private readonly IDbConnectionTester _connectionTester;

    private readonly ISettingsService _settingsService;

    private readonly SettingsModel _settings;

    public ConnectionViewModel(
        IDbConnectionTester connectionTester,
        IGrafikService grafikService,
        SettingsModel settings,
        ISettingsService settingsService
    )
    {
        _connectionTester = connectionTester;
        _settings = settings;
        _settingsService = settingsService;

        ConnectionString = settings.DbConnectionModel.ConnectionString;
        SelectedProvider = settings.DbConnectionModel.ProviderType;
        HeartPulseIcon = grafikService.GetIcon(AppIcon.HeartPulse, AppBrush.PrimaryForeground);
    }

    public event Action<bool, string?>? ConnectionTestCompleted;

    public event Action<SettingsModel>? SettingsSaved;

    public IImage HeartPulseIcon { get; }

    public IEnumerable<ProviderType> ProviderOptions { get; } = Enum.GetValues<ProviderType>();

    [ObservableProperty]
    public partial string ConnectionString { get; set; }

    [ObservableProperty]
    public partial ProviderType SelectedProvider { get; set; }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        _settings.DbConnectionModel.ConnectionString = ConnectionString;
        _settings.DbConnectionModel.ProviderType = SelectedProvider;
        _settingsService.Save(_settings);
        SettingsSaved?.Invoke(_settings);
        SaveCommand.NotifyCanExecuteChanged();
    }

    private bool CanSave() =>
        ConnectionString != _settings.DbConnectionModel.ConnectionString
        || SelectedProvider != _settings.DbConnectionModel.ProviderType;

    partial void OnConnectionStringChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

    partial void OnSelectedProviderChanged(ProviderType value) => SaveCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private void TestConnection()
    {
        DbConnectionModel dbConnection = new DbConnectionModel
        {
            ConnectionString = ConnectionString,
            ProviderType = SelectedProvider,
        };

        _connectionTester
            .Run(dbConnection)
            .Match<object?>(
                _ =>
                {
                    ConnectionTestCompleted?.Invoke(true, null);
                    return null;
                },
                error =>
                {
                    ConnectionTestCompleted?.Invoke(false, error.Value.Message);
                    return null;
                }
            );
    }
}
