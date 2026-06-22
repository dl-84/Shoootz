using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Result;
using Shoootz.Models.Error;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Database;
using Shoootz.Models.Settings.Udp;
using Shoootz.Services.Data;
using Shoootz.Services.Graphics;
using Shoootz.Services.Language;
using Shoootz.Services.License;
using Shoootz.Services.Localization;
using Shoootz.Services.Parser;
using Shoootz.Services.Settings;
using Shoootz.Services.Settings.Read;
using Shoootz.Services.Settings.Validation;
using Shoootz.Services.Store;
using Shoootz.Services.Udp;
using Shoootz.Store;
using Shoootz.Store.Services;
using Shoootz.ViewModels;
using Shoootz.ViewModels.Competition;
using Shoootz.ViewModels.Info;
using Shoootz.ViewModels.Settings;
using Shoootz.Views;
using Themes.Disag;

namespace Shoootz;

/// <inheritdoc />
public class App : Application
{
    private readonly List<SettingsError> _errors = [];

    private IConfigurationRoot _configurationRoot = new ConfigurationRoot(new List<IConfigurationProvider>());

    private string _rawSettings = string.Empty;

    /// <inheritdoc/>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Styles.Add(new Disag());
    }

    /// <inheritdoc/>
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
            mainWindowViewModel.CheckDbConnection();
            UdpConnection? udpConnection = _configurationRoot.Get<SettingsModel>()?.UdpConnection;

            if (udpConnection is not null && udpConnection.AutoConnect)
            {
                serviceProvider.GetRequiredService<IUdpListenerService>().Start(udpConnection.Port);
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
        services.AddSingleton<ILicenseService, LicenseService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<IShotDataParser, ShotDataParser>();
        services.AddSingleton<ISettingsWriter, SettingsWriter>();
        services.AddSingleton<IUdpListenerService, UdpListenerService>();
        services.AddSingleton<MainWindowViewModel>();
    }

    private static void InitTransient(ServiceCollection services)
    {
        services.AddTransient<AboutViewModel>();
        services.AddTransient<ConnectionViewModel>();
        services.AddTransient<EvaluationViewModel>();
        services.AddTransient<GeneralViewModel>();
        services.AddTransient<GroupsViewModel>();
        services.AddTransient<LicensesViewModel>();
    }

    private void InitContext(ServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContextFactory<AppDbContext>(options =>
        {
            DatabaseConnection? databaseConnection = _configurationRoot.Get<SettingsModel>()?.DatabaseConnection;

            switch (databaseConnection?.Provider)
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
        });

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

        if (_errors.Count == 0 && _configurationRoot.Get<SettingsModel>()?.DatabaseConnection is not null)
        {
            InitContext(serviceCollection);
        }

        return serviceCollection.BuildServiceProvider();
    }

    private void InitSettings(ServiceCollection serviceCollection)
    {
        SettingsReader.Read.Match(value => _rawSettings = value, error => _errors.Add(error.Value));

        if (_errors.Count > 0)
        {
            return;
        }

        Result<string, List<SettingsError>> validationResult = SettingsValidation.Run(_rawSettings);

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
