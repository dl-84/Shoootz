using Shoootz.Common.Services.License;
using Shoootz.Common.Services.Localization;
using Shoootz.Common.ViewModels.Info;

namespace Shoootz.ViewModels.Info;

internal class LicensesViewModel(ILicenseService licenseService, ILocalizationService localizationService)
    : InfoViewModelBase(localizationService, licenseService.GetThirdPartyPackages()) { }
