using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shoootz.Context;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Database;
using Shoootz.Services.Database;
using Shoootz.Services.Grafik;
using Shoootz.Services.Language;
using Shoootz.Services.License;
using Shoootz.Services.Localization;
using Shoootz.Services.Settings;
using Shoootz.ViewModels;
using Shoootz.Views;
using Themes.Disag;

namespace Shoootz;

/// <inheritdoc />
public class App : Application
{
    private static readonly SettingsService _settingsService = new SettingsService();

    private IServiceProvider? _serviceProvider;

    /// <inheritdoc/>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Styles.Add(new Disag());
    }

    /// <inheritdoc/>
    public override void OnFrameworkInitializationCompleted()
    {
        SettingsModel? settings = ReadSettings(out List<SettingsError>? settingsErrors);
        _serviceProvider = InitServiceProvider(settings);

        MainWindowViewModel mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        ILocalizationService localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();

        localizationService.SetLanguage(settings?.CurrentLanguageCode ?? "de");

        if (settings is not null)
        {
            mainWindowViewModel.InitSettings(settings);
            mainWindowViewModel.CheckDbConnection();
            _serviceProvider.GetRequiredService<IDbService>().InitializeAsync().GetAwaiter().GetResult();
        }

        if (settingsErrors is not null)
        {
            mainWindowViewModel.RedirectToSettingsError(settingsErrors);
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = mainWindowViewModel };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider InitServiceProvider(SettingsModel? settings)
    {
        ServiceCollection services = new ServiceCollection();
        InitSingletons(services);

        if (settings?.DbConnectionModel is not null)
        {
            InitContext(services, settings.DbConnectionModel);
        }

        return services.BuildServiceProvider();
    }

    private static void InitContext(ServiceCollection services, DbConnectionModel dbConnection)
    {
        services.AddDbContextFactory<AppDbContext>(options =>
        {
            switch (dbConnection.ProviderType)
            {
                case ProviderType.PostgreSql:
                    options.UseNpgsql(dbConnection.ConnectionString);
                    break;

                case ProviderType.Sqlite:
                default:
                    options.UseSqlite(dbConnection.ConnectionString);
                    break;
            }
        });

        services.AddSingleton<IDbService, DbService>();
    }

    private static void InitSingletons(ServiceCollection services)
    {
        services.AddSingleton<IDbConnectionTester, DbConnectionTester>();
        services.AddSingleton<IGrafikService, GrafikService>();
        services.AddSingleton<ILanguageService, LanguageService>();
        services.AddSingleton<ILicenseService, LicenseService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<ISettingsService>(_settingsService);
        services.AddSingleton<MainWindowViewModel>();
    }

    private static SettingsModel? ReadSettings(out List<SettingsError>? settingsErrors)
    {
        List<SettingsError>? errorList = null;
        SettingsModel? settings = _settingsService
            .Load()
            .Match<SettingsModel?>(
                settings => settings,
                errors =>
                {
                    errorList = errors.Value;
                    return null;
                }
            );

        settingsErrors = errorList;
        return settings;
    }
}
