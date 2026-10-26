using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Result;
using Shoootz.Bridge.TCP.Models.Settings;
using Shoootz.Bridge.TCP.Services.Bridge;
using Shoootz.Bridge.TCP.ViewModels;
using Shoootz.Bridge.TCP.ViewModels.General;
using Shoootz.Bridge.TCP.ViewModels.Info;
using Shoootz.Bridge.TCP.ViewModels.Settings;
using Shoootz.Bridge.TCP.Views;
using Shoootz.Common.Models.Language.Error;
using Shoootz.Common.Services.App;
using Shoootz.Common.Services.Graphics;
using Shoootz.Common.Services.Language;
using Shoootz.Common.Services.License;
using Shoootz.Common.Services.Localization;
using Shoootz.Common.Services.Settings;

namespace Shoootz.Bridge.TCP;

public class App : Application
{
    private readonly List<SettingsError> _errors = [];

    private IConfigurationRoot _configurationRoot = new ConfigurationRoot(new List<IConfigurationProvider>());

    private string _rawSettings = string.Empty;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        IServiceProvider serviceProvider = InitServiceProvider();
        SettingsModel? settingsModel = _configurationRoot.Get<SettingsModel>();
        InitLocalization(serviceProvider, settingsModel);
        MainWindowViewModel mainWindowViewModel = serviceProvider.GetRequiredService<MainWindowViewModel>();

        if (_errors.Count > 0)
        {
            mainWindowViewModel.RedirectToSettingsError(_errors, _rawSettings);
        }
        else if (settingsModel is { AutoConnect: true })
        {
            serviceProvider.GetRequiredService<IBridgeService>().Start();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = mainWindowViewModel };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void InitSingletons(ServiceCollection services)
    {
        services.AddSingleton<IBridgeService, BridgeService>();
        services.AddSingleton<IGraphicsService, GraphicsService>();
        services.AddSingleton<ILanguageService, LanguageService>();
        services.AddSingleton<ILicenseService>(_ => new LicenseService(typeof(App).Assembly));
        services.AddSingleton<ILocalizationService>(_ => new LocalizationService(
            "Shoootz.Common.Resources.Lang.Messages",
            typeof(LocalizationService).Assembly
        ));
        services.AddSingleton<ISettingsWriter<SettingsModel>, SettingsWriter<SettingsModel>>();
        services.AddSingleton<MainWindowViewModel>();
    }

    private static void InitTransient(ServiceCollection services)
    {
        services.AddTransient<AboutViewModel>();
        services.AddTransient<AppViewModel>();
        services.AddTransient<GeneralViewModel>();
        services.AddTransient<LicensesViewModel>();
    }

    private static void InitLocalization(IServiceProvider serviceProvider, SettingsModel? settingsModel)
    {
        ILocalizationService localizationService = serviceProvider.GetRequiredService<ILocalizationService>();
        LocalizationService.Register(localizationService);
        localizationService.SetLanguage(settingsModel?.CurrentLanguageCode ?? "de");
    }

    private ServiceProvider InitServiceProvider()
    {
        ServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddOptions();

        InitSettings(serviceCollection);
        InitSingletons(serviceCollection);
        InitTransient(serviceCollection);

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
            "Shoootz.Bridge.TCP.Resources.JsonSchemas.Settings.schema.json"
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
