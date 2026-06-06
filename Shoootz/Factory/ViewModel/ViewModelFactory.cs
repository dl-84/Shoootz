using System;
using Microsoft.Extensions.DependencyInjection;
using Shoootz.Models.Settings;
using Shoootz.Services.Graphics;
using Shoootz.Services.Language;
using Shoootz.Services.License;
using Shoootz.Services.Localization;
using Shoootz.Services.Settings;
using Shoootz.Services.Store;
using Shoootz.Services.Udp;
using Shoootz.Store.Services;
using Shoootz.ViewModels;
using Shoootz.ViewModels.Competition;
using Shoootz.ViewModels.Info;
using Shoootz.ViewModels.Settings;

namespace Shoootz.Factory.ViewModel;

internal class ViewModelFactory(
    IConnectionTester connectionTester,
    IGraphicsService graphicsService,
    ILanguageService languageService,
    ILicenseService licenseService,
    ILocalizationService localizationService,
    IServiceProvider serviceProvider,
    ISettingsService settingsService,
    IUdpListenerService udpListenerService
) : IViewModelFactory
{
    public ViewModelBase CreateView(int index) =>
        index switch
        {
            1 => new EvaluationViewModel(),
            3 => new GeneralViewModel(
                languageService,
                localizationService,
                settingsService.CurrentSettings ?? new SettingsModel(),
                settingsService
            ),
            4 => new ConnectionViewModel(
                connectionTester,
                graphicsService,
                settingsService.CurrentSettings ?? new SettingsModel(),
                settingsService,
                serviceProvider.GetRequiredService<IStoreService>(),
                udpListenerService
            ),
            5 => new GroupsViewModel(),
            7 => new AboutViewModel(licenseService, localizationService),
            8 => new LicensesViewModel(licenseService, localizationService),
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
        };
}
