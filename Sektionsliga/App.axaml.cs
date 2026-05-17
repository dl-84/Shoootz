using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Sektionsliga.Services.Flag;
using Sektionsliga.Services.Settings;
using Sektionsliga.ViewModels;
using Sektionsliga.Views;

namespace Sektionsliga;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ServiceProvider serviceProvider = InitServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider InitServiceProvider()
    {
        ServiceCollection services = new ServiceCollection();

        InitSingletons(services);

        return services.BuildServiceProvider();
    }

    private static ServiceCollection InitSingletons(ServiceCollection services)
    {
        services.AddSingleton<IFlagService, FlagService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<MainWindowViewModel>();

        return services;
    }
}
