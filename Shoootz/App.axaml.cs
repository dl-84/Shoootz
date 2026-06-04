using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shoootz.Models.Error;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Database;
using Shoootz.Services.Data;
using Shoootz.Services.Graphics;
using Shoootz.Services.Language;
using Shoootz.Services.License;
using Shoootz.Services.Localization;
using Shoootz.Services.Parser;
using Shoootz.Services.Settings;
using Shoootz.Services.Store;
using Shoootz.Services.Udp;
using Shoootz.Store;
using Shoootz.Store.Services;
using Shoootz.ViewModels;
using Shoootz.Views;
using Themes.Disag;

namespace Shoootz;

/// <inheritdoc />
public class App : Application
{
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
        SettingsService settingsService = new SettingsService();
        SettingsModel? settings = ReadSettings(settingsService, out List<SettingsError>? settingsErrors);
        _serviceProvider = InitServiceProvider(settings, settingsService);

        MainWindowViewModel mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

        ILocalizationService localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
        LocalizationService.Register(localizationService);
        localizationService.SetLanguage(settings?.CurrentLanguageCode ?? "de");

        if (settingsErrors is not null)
        {
            mainWindowViewModel.RedirectToSettingsError(settingsErrors);
        }
        else
        {
            mainWindowViewModel.InitSettings(settings!);
            mainWindowViewModel.CheckDbConnection();
            _serviceProvider.GetRequiredService<IStoreService>().InitializeAsync().GetAwaiter().GetResult();

            if (settings!.UdpConnectionModel.AutoConnect)
            {
                _serviceProvider
                    .GetRequiredService<IUdpListenerService>()
                    .Start(settings.UdpConnectionModel.IpAddress, settings.UdpConnectionModel.Port);
            }
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = mainWindowViewModel };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider InitServiceProvider(SettingsModel? settings, SettingsService settingsService)
    {
        ServiceCollection services = new ServiceCollection();
        InitSingletons(services, settingsService);

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
                    options.UseNpgsql(
                        dbConnection.ConnectionString,
                        optionsBuilder => optionsBuilder.MigrationsAssembly("Shoootz.Store.Adapter.PostgreSQL")
                    );
                    break;

                case ProviderType.Sqlite:
                default:
                    options.UseSqlite(
                        dbConnection.ConnectionString,
                        optionsBuilder => optionsBuilder.MigrationsAssembly("Shoootz.Store.Adapter.SQLite")
                    );
                    break;
            }
        });

        services.AddSingleton<IStoreService, StoreService>();
    }

    private static void InitSingletons(ServiceCollection services, SettingsService settingsService)
    {
        services.AddSingleton<IDataProcessor, DataProcessor>();
        services.AddSingleton<IConnectionTester, ConnectionTester>();
        services.AddSingleton<IGraphicsService, GraphicsService>();
        services.AddSingleton<ILanguageService, LanguageService>();
        services.AddSingleton<ILicenseService, LicenseService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<ISettingsService>(settingsService);
        services.AddSingleton<IShotDataParser, ShotDataParser>();
        services.AddSingleton<IUdpListenerService, UdpListenerService>();
        services.AddSingleton<MainWindowViewModel>();
    }

    private static SettingsModel? ReadSettings(SettingsService settingsService, out List<SettingsError>? settingsErrors)
    {
        List<SettingsError>? errorList = null;
        SettingsModel? loadedSettings = null;

        settingsService.Load().Match(
            settings => loadedSettings = settings,
            errors => errorList = errors.Value
        );

        settingsErrors = errorList;
        return loadedSettings;
    }
}
