using Shoootz.Services.License;
using Shoootz.Services.Localization;

namespace Shoootz.ViewModels.Info;

internal class LicensesViewModel(ILicenseService licenseService, ILocalizationService localizationService)
    : InfoViewModelBase(localizationService, licenseService.GetThirdPartyPackages()) { }
