using Shoootz.Common.Services.License;
using Shoootz.Common.Services.Localization;
using Shoootz.Common.ViewModels.Info;

namespace Shoootz.Bridge.TCP.ViewModels.Info;

internal class AboutViewModel(ILicenseService licenseService, ILocalizationService localizationService)
    : InfoViewModelBase(localizationService, licenseService.GetAppPackages()) { }
