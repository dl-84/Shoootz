using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Controls.LicenseTable.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Result;
using Shoootz.Common.Models.Language.Error;
using Shoootz.Common.Services.App;
using Shoootz.Common.Services.Graphics;
using Shoootz.Common.Services.Language;
using Shoootz.Common.Services.License;
using Shoootz.Common.Services.Localization;
using Shoootz.Common.Services.Settings;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Database;
using Shoootz.Models.Settings.Incoming;
using Shoootz.Services.Data;
using Shoootz.Services.Parser;
using Shoootz.Services.Store;
using Shoootz.Services.Udp;
using Shoootz.Store;
using Shoootz.Store.Services;
using Shoootz.Store.Types;
using Shoootz.ViewModels;
using Shoootz.ViewModels.Competition;
using Shoootz.ViewModels.Info;
using Shoootz.ViewModels.Settings;
using Shoootz.Views;
using Themes.Disag;

namespace Shoootz;

public class App : Application
{
    private readonly List<SettingsError> _errors = [];

    private IConfigurationRoot _configurationRoot = new ConfigurationRoot(new List<IConfigurationProvider>());

    private string _rawSettings = string.Empty;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Styles.Add(new Disag());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        IServiceProvider serviceProvider = InitServiceProvider();
        InitLocalization(serviceProvider);
        MainWindowViewModel mainWindowViewModel = serviceProvider.GetRequiredService<MainWindowViewModel>();

        if (_errors.Count > 0)
        {
            mainWindowViewModel.RedirectToSettingsError(_errors, _rawSettings);
        }
        else
        {
            _ = mainWindowViewModel.CheckDbConnectionAsync();
            Receiver? udpConnection = _configurationRoot.Get<SettingsModel>()?.Receiver;

            if (udpConnection is not null && udpConnection.AutoConnect)
            {
                serviceProvider.GetRequiredService<IDataProcessor>().Start();
                _ = serviceProvider.GetRequiredService<IUdpListenerService>().Start(udpConnection.Port);
            }
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = mainWindowViewModel };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void InitSingletons(ServiceCollection services)
    {
        services.AddSingleton<IConnectionTester, ConnectionTester>();
        services.AddSingleton<IDataProcessor, DataProcessor>();
        services.AddSingleton<IGraphicsService, GraphicsService>();
        services.AddSingleton<ILanguageService, LanguageService>();
        services.AddSingleton<ILicenseService>(serviceProvider => new LicenseService(
            typeof(App).Assembly,
            () =>
                [
                    new PackageModel(
                        serviceProvider.GetRequiredService<ILocalizationService>()["Database"],
                        serviceProvider.GetService<IStoreService>()?.DbVersion ?? "n/a",
                        LicenseService.LicenseType,
                        null,
                        null,
                        LicenseService.Copyright
                    ),
                ]
        ));
        services.AddSingleton<ILocalizationService>(_ => new LocalizationService(
            "Shoootz.Common.Resources.Lang.Messages",
            typeof(LocalizationService).Assembly
        ));
        services.AddSingleton<IShotDataParser, ShotDataParser>();
        services.AddSingleton<ISettingsWriter<SettingsModel>, SettingsWriter<SettingsModel>>();
        services.AddSingleton<IUdpListenerService, UdpListenerService>();
        services.AddSingleton<MainWindowViewModel>();
    }

    private static void InitTransient(ServiceCollection services)
    {
        services.AddTransient<AboutViewModel>();
        services.AddTransient<ClubsViewModel>();
        services.AddTransient<ConnectionViewModel>();
        services.AddTransient<EvaluationViewModel>();
        services.AddTransient<GroupsViewModel>();
        services.AddTransient<LicensesViewModel>();
        services.AddTransient<LoggingViewModel>();
        services.AddTransient<ShootersViewModel>();
        services.AddTransient<ShotsViewModel>();
    }

    private void InitContext(ServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContextFactory<AppDbContext>(
            (serviceProvider, options) =>
            {
                DatabaseConnection? databaseConnection = serviceProvider
                    .GetRequiredService<IOptionsMonitor<SettingsModel>>()
                    .CurrentValue.DatabaseConnection;

                switch (databaseConnection.Provider)
                {
                    case ProviderType.PostgreSql:
                        options.UseNpgsql(
                            databaseConnection.ConnectionString,
                            optionsBuilder => optionsBuilder.MigrationsAssembly("Shoootz.Store.Adapter.PostgreSQL")
                        );
                        break;

                    case ProviderType.Sqlite:
                    default:
                        options.UseSqlite(
                            databaseConnection?.ConnectionString,
                            optionsBuilder => optionsBuilder.MigrationsAssembly("Shoootz.Store.Adapter.SQLite")
                        );
                        break;
                }
            }
        );

        serviceCollection.AddSingleton<IStoreService, StoreService>();
    }

    private void InitLocalization(IServiceProvider serviceProvider)
    {
        ILocalizationService localizationService = serviceProvider.GetRequiredService<ILocalizationService>();
        LocalizationService.Register(localizationService);
        localizationService.SetLanguage(_configurationRoot.Get<SettingsModel>()?.CurrentLanguageCode ?? "de");
    }

    private ServiceProvider InitServiceProvider()
    {
        ServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddOptions();

        InitSettings(serviceCollection);
        InitSingletons(serviceCollection);
        InitTransient(serviceCollection);

        InitContext(serviceCollection);

        return serviceCollection.BuildServiceProvider();
    }

    private void InitSettings(ServiceCollection serviceCollection)
    {
        if (!File.Exists(AppPath.SettingsFile))
        {
            new SettingsWriter<SettingsModel>().Save(new SettingsModel());
        }

        SettingsReader.Read.Match(value => _rawSettings = value, error => _errors.Add(error.Value));

        if (_errors.Count > 0)
        {
            return;
        }

        Result<string, List<SettingsError>> validationResult = SettingsValidation.Run(
            _rawSettings,
            typeof(App).Assembly,
            "Shoootz.Resources.JsonSchemas.Settings.schema.json"
        );

        validationResult.Match(
            value =>
            {
                _configurationRoot = new ConfigurationBuilder()
                    .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(value)))
                    .Build();

                serviceCollection.AddSingleton<IConfigurationRoot>(_configurationRoot);
                serviceCollection.Configure<SettingsModel>(_configurationRoot);
            },
            error => _errors.AddRange(error.Value)
        );
    }
}
