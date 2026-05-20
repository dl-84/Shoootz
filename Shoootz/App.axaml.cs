using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Shoootz.Models;
using Shoootz.Services.Grafik;
using Shoootz.Services.Language;
using Shoootz.Services.Localization;
using Shoootz.Services.Settings;
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
        _serviceProvider = InitServiceProvider();

        MainWindowViewModel mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        ILocalizationService localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();

        SettingsModel? settings = ReadSettings(out List<SettingsError>? settingsErrors);
        localizationService.SetLanguage(settings?.CurrentLanguageCode ?? "en");

        if (settings is not null)
        {
            mainWindowViewModel.InitSettings(settings);
        }

        if (settingsErrors is not null)
        {
            mainWindowViewModel.RedirectToSettings(settingsErrors);
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = mainWindowViewModel };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider InitServiceProvider()
    {
        ServiceCollection services = new ServiceCollection();
        InitSingletons(services);
        return services.BuildServiceProvider();
    }

    private static void InitSingletons(ServiceCollection services)
    {
        services.AddSingleton<IGrafikService, GrafikService>();
        services.AddSingleton<ILanguageService, LanguageService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<MainWindowViewModel>();
    }

    private SettingsModel? ReadSettings(out List<SettingsError>? settingsErrors)
    {
        ISettingsService settingsService = _serviceProvider!.GetRequiredService<ISettingsService>();

        List<SettingsError>? errorList = null;
        SettingsModel? settings = settingsService
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
